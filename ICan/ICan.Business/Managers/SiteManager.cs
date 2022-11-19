using AutoMapper;
using ICan.Business.Services;
using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using ICan.Common.Utils;
using ICan.Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUglify;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class SiteManager : BaseManager
	{
		private readonly S3FileManager _s3FileManager;
		private readonly ISiteRepository _siteRepository;
		private readonly IProductRepository _productRepository;
		private readonly CloudConfiguration _cloudConfiguration;
		private readonly JwtGeneratorService _jwtGeneratorService;
		private readonly IUserRepository _userRepository;
		private readonly HttpRequestSenderService _httpRequestSenderService;

		public SiteManager(
			ApplicationDbContext context,
			S3FileManager s3FileManager,
			IProductRepository productRepository,
			ISiteRepository siteRepository,
			IMapper mapper,
			ILogger<SiteManager> logger,
			IConfiguration configuration,
			JwtGeneratorService jwtGeneratorService,
			IUserRepository userRepository,
			HttpRequestSenderService httpRequestSenderService,
			IOptions<CloudConfiguration> cloudConfig) : base(mapper, context, logger, configuration)
		{
			_s3FileManager = s3FileManager;
			_siteRepository = siteRepository;
			_productRepository = productRepository;
			_cloudConfiguration = cloudConfig.Value;
			_jwtGeneratorService = jwtGeneratorService;
			_userRepository = userRepository;
			_httpRequestSenderService = httpRequestSenderService;
		}

		public IEnumerable<SiteProductModel> GetAll(int siteId, ClientFilter siteFilter)
		{
			var rawList = _siteRepository.GetByFilterAndTag(siteId, siteFilter).ToList();
			var list = MapToModel(rawList, siteId);
			return list;
		}

		public async Task SetBotDescription()
		{
			var rawList = await _productRepository.GetNotebooks().ToListAsync();
			rawList.ForEach(item =>
			{
				item.BotDescription = string.IsNullOrWhiteSpace(item.Description) ? string.Empty : Uglify.HtmlToText(item.Description).ToString();
				item.BotInformation = string.IsNullOrWhiteSpace(item.Information) ? string.Empty : Uglify.HtmlToText(item.Information).ToString();
			});

			await _productRepository.UpdateRangeAsync(rawList);
		}

		public async Task<SiteProductModel> GetAsync(int siteId, int id, bool showAllSitesData = false)
		{
			var raw = await _siteRepository.GetById(id);
			if (raw == null)
			{
				_logger.LogWarning($"[GetNotebook][ERROR] failed siteId {siteId}, id {id}");
				return null;
			}
			var model = _mapper.Map<SiteProductModel>(raw);
			model.BucketUrl = _cloudConfiguration.BucketUrl;
			model.ProductImages = model.ProductImages?
				.OrderBy(img => img.ImageType)
				  .ThenBy(img => img.Order);
			SetMarketplaceproducts(siteId, raw, model, showAllSitesData);
			return model;
		}

		public async Task<Dictionary<OptProductseries, List<SiteProductModel>>> GetSiteProductsAsync()
		{
			var raw = _productRepository.GetNotebooks();

			var products = _mapper.Map<IEnumerable<SiteProductModel>>(raw).ToList();

			var seriesIds = products.Select(t => t.ProductSeriesId).Distinct();
			var series = await _context.OptProductseries
				.Where(t => seriesIds.Contains(t.ProductSeriesId))
				.OrderBy(t => t.ProductKindId).ThenBy(t => t.Order).ToListAsync();
			series.Add(new OptProductseries { ProductSeriesId = 0, Name = "" });

			var result = new Dictionary<OptProductseries, List<SiteProductModel>>();
			foreach (var item in series)
			{
				var values = products.Where(t => t.ProductSeriesId == item.ProductSeriesId)
					.OrderBy(product => (product.DisplayOrder ?? 0))
					.ToList();
				if (values.Any())
				{
					result.Add(item, values);
				}
			}

			return result;
		}

		public bool CatalogImageExists(int productId)
		{
			var catalogImageExists = _context.OptProductImage
				.Any(img => img.ProductId == productId && img.ImageType == (int)ProductImageType.Catalog);
			return catalogImageExists;
		}

		public async Task ParseFileAsync(IFormFile file)
		{
			var list = new List<OptMarketplaceProduct>();
			var products = _productRepository.GetNotebooks();
			using (var memoryStream = new MemoryStream())
			{
				await file.CopyToAsync(memoryStream).ConfigureAwait(false);

				using (var package = new ExcelPackage(memoryStream))
				{
					foreach (var worksheet in package.Workbook.Worksheets)
					{
						list.AddRange(ReadSheet(products, worksheet));
					}

					await _siteRepository.AddMarketPlaceInfoAsync(list);
				}
			}
		}

		public async Task UpdateAsync(SiteProductModel model, IFormFile video)
		{
			var raw = _productRepository.GetDetails(model.ProductId);
			if (video != null)
			{
				if (!string.IsNullOrWhiteSpace(raw.VideoFileName))
				{
					await _s3FileManager.RemoveOldFileAsync(raw.VideoFileName);
				}
				var fileName = await _s3FileManager.SaveFileAsync(video);
				raw.VideoFileName = fileName;
			}

			raw.SiteName = model.SiteName;
			raw.Information = model.Information;
			raw.Description = model.Description;
			raw.LongDescription = model.LongDescription;
			raw.ReviewsText = model.ReviewsText;
			raw.BotInformation = model.BotInformation;
			raw.BotDescription = model.BotDescription;

			await _productRepository.UpdateAsync(raw);
		}

		public async Task AddImageAsync(ProductImageModel model, IFormFile image)
		{
			var fileName = await _s3FileManager.SaveFileAsync(image);
			var raw = new OptProductImage
			{
				UserFileName = image.FileName,
				FileName = fileName,
				ImageType = (int)model.ImageType,
				Order = model.Order,
				ProductId = model.ProductId,
			};
			await _context.AddAsync(raw);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteImage(int imageId)
		{
			var raw = _context.OptProductImage.Find(imageId);
			await _s3FileManager.RemoveOldFileAsync(raw.FileName);
			_context.Remove(raw);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteVideo(int productId)
		{
			var raw = _productRepository.GetDetails(productId);
			await _s3FileManager.RemoveOldFileAsync(raw.VideoFileName);
			raw.VideoFileName = string.Empty;
			await _productRepository.UpdateAsync(raw);
		}

		public async Task RunJob(string userId, string jobName)
		{
			var user = await _userRepository.GetUserById(userId);
			var token = "Bearer " + _jwtGeneratorService.CreateToken(user);
			var response = await _httpRequestSenderService.SendRequest("", _configuration["Jobs:JobRunnerUrl"] + jobName,
				requestType: RequestType.POST, headers: new Dictionary<string, string> {
					{ "Authorization", token }
			});

			if (!response.IsSuccessStatusCode)
				throw new UserException(Const.ErrorMessages.CantStart);
		}

		public IEnumerable<int> GetProductTagIds(int productId)
		{
			var productTagIds = _siteRepository.GetProductTagIds(productId);
			return productTagIds.ToList();
		}

		public async Task AddTagAsync(ProductTagModel model)
		{
			var raw = _mapper.Map<OptProductTag>(model);
			await _context.AddAsync(raw);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteTag(int productTagId)
		{
			await _siteRepository.DeleteTag(productTagId);
		}

		public async Task AddMarketPlaceProductAsync(MarketplaceProductModel model)
		{
			var raw = _mapper.Map<OptMarketplaceProduct>(model);

			await _siteRepository.AddMarketPlaceProductAsync(raw);
		}

		public async Task UpdateMarketPlaceProductAsync(MarketplaceProductModel model)
		{
			var raw = _mapper.Map<OptMarketplaceProduct>(model);
			await _siteRepository.UpdateMarketPlaceProductAsync(raw);
		}

		public MarketplaceProductModel GetMarketplaceProduct(int marketPlaceProductId)
		{
			var raw = _siteRepository.GetMarketPlaceProduct(marketPlaceProductId);
			var model = _mapper.Map<MarketplaceProductModel>(raw);
			return model;
		}

		public IEnumerable<MarketplaceModel> GetAvailableMarketplaces(int productId)
		{
			var rawList = _siteRepository.GetAvailableMarketplaces(productId);
			return _mapper.Map<IEnumerable<MarketplaceModel>>(rawList);
		}

		public async Task DeleteMarketplaceProductAsync(int marketPlaceProductId)
		{
			await _siteRepository.DeleteMarketplaceProductAsync(marketPlaceProductId);
		}

		public string GetLocale(int currentSiteId)
		{
			return _siteRepository.GetLocale(currentSiteId);
		}

		private IEnumerable<SiteProductModel> MapToModel(IEnumerable<OptProduct> rawList, int siteId)
		{
			var list = rawList.Select(product =>
			{
				var model = new SiteProductModel();

				model.ProductId = product.ProductId;
				model.Name = product.Name;
				model.SiteName = product.SiteName;
				model.BucketUrl = _cloudConfiguration.BucketUrl;
				SetMarketplaceproducts(siteId, product, model);

				model.ProductImages = _mapper.Map<IEnumerable<ProductImageModel>>(product.ProductImages);
				return model;
			});
			return list;
		}

		private void SetMarketplaceproducts(int siteId, OptProduct product, SiteProductModel model, bool showAllSitesData = false)
		{
			if (product.MarketplaceProducts != null && product.MarketplaceProducts.Any())
			{
				var productList = new List<MarketplaceProductModel>();
				var marketPlaceProducts = showAllSitesData
					? product.MarketplaceProducts
					: product.MarketplaceProducts.Where(marketPlaceProd => marketPlaceProd.Marketplace.SiteId == siteId
						&& marketPlaceProd.ShowOnSite);

				foreach (var item in marketPlaceProducts)
				{
					var mProductModel = GetModel(siteId);
					_mapper.Map(item, mProductModel);
					productList.Add(mProductModel);
				}
				model.MarketplaceProducts = productList;
			}
		}

		private MarketplaceProductModel GetModel(int siteId)
		{
			switch (siteId)
			{
				case 2:
					return new UkMarketplaceProductModel();
				default:
					return new MarketplaceProductModel();
			}
		}

		private IEnumerable<OptMarketplaceProduct> ReadSheet(IOrderedQueryable<OptProduct> products, ExcelWorksheet worksheet)
		{
			var marketPlaceId = _siteRepository.FindMarketPlaceByName(worksheet.Name);
			if (!marketPlaceId.HasValue)
			{
				throw new UserException($"Для листа с названием {worksheet.Name} не определён маркетлейс");
			}

			var list = new List<OptMarketplaceProduct>();
			for (var i = 2; i <= worksheet.Dimension.Rows; i++)
			{
				var isbn = worksheet.Cells[i, 2].Value?.ToString();
				if (string.IsNullOrWhiteSpace(isbn))
				{
					continue;
				}
				var productId = products.FirstOrDefault(prod =>
					!string.IsNullOrWhiteSpace(prod.ISBN) && prod.ISBN.Equals(isbn))
					?.ProductId;

				if (!productId.HasValue)
				{
					throw new UserException($"Для листа с названием {worksheet.Name} не найдена тетрадь с isbn {isbn}");
				}
				var product = new OptMarketplaceProduct
				{
					MarketplaceId = marketPlaceId.Value,
					ProductId = productId.Value,
					Price = decimal.TryParse(worksheet.Cells[i, 3].Value?.ToString(), out var price) ? price : (decimal?)null,
					ReviewsAmount = int.TryParse(worksheet.Cells[i, 4].Value?.ToString(), out var reviewsCount) ? reviewsCount : (int?)null,
					Raiting = float.TryParse(worksheet.Cells[i, 5].Value?.ToString(), out var rate) ? rate : (float?)null,
				};
				var urls = worksheet.Cells[i, 6].Value?.ToString();
				if (!string.IsNullOrWhiteSpace(urls))
				{
					var parseUrls = urls.Split(";");
					product.Urls = parseUrls.Select(url => new OptMarketplaceProductUrl { Url = url?.Trim().ToLower() }).ToList();
				}
				list.Add(product);
			}
			return list;
		}
	}
}
