using AutoMapper;
using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class ProductManager : BaseManager
	{
		private readonly EventManager _eventManager;
		private readonly IProductRepository _productRepository;
		private readonly IPriceRepository _priceRepository;

		public ProductManager(IMapper mapper,
			IProductRepository productRepository,
			IPriceRepository priceRepository,
			ApplicationDbContext context,
			 EventManager eventManager,
			ILogger<BaseManager> logger) : base(mapper, context, logger)
		{
			_eventManager = eventManager;
			_productRepository = productRepository;
			_priceRepository = priceRepository;
		}

		public async Task<Dictionary<int, IdIsbnNameProductModel>> GetProductsWithIsbn()
		{
			var result = new Dictionary<int, IdIsbnNameProductModel>();
			await _context.OptProduct.Where(notebook => notebook.ProductKindId == 1 /*Тетрадь*/
					 && !string.IsNullOrWhiteSpace(notebook.ISBN) && notebook.ISBN.Length > 3)
				.ForEachAsync(notebook =>
					result.Add(notebook.ProductId,
						new IdIsbnNameProductModel
						{
							ProductId = notebook.ProductId,
							Name = notebook.Name,
							ISBN = notebook.ISBN,
							ArticleNumber = notebook.ArticleNumber
						}
						));
			return result;
		}

		public IEnumerable<ProductModel> GetNotebooks()
		{
			var raw = _productRepository.GetNotebooks();
			var list = _mapper.Map<IEnumerable<ProductModel>>(raw);
			return list;
		}

		public async Task<Dictionary<OptProductseries, List<ProductModel>>> GetAsync(bool onlyWithPrices,
			Guid? orderId = null, bool dontShowDisabled = true, bool onlyNotebooks = false)
		{
			var raw = _productRepository.GetProducts();

			var now = orderId == null
				 ? DateTime.Now
				 : _context.OptOrder.First(t => t.OrderId == orderId.Value).OrderDate;

			var prices = _priceRepository.GetPrices(now);

			var productIdsWithPrice = prices.Select(t => t.ProductId).Distinct();
			var products = await raw.OrderBy(t => t.ProductId).ToListAsync();

			if (dontShowDisabled)
				products = products.Where(t => t.Enabled).ToList();

			if (onlyWithPrices)
			{
				products = products.Where(prod => productIdsWithPrice.Contains(prod.ProductId)).ToList();
			}

			if (onlyNotebooks)
			{
				products = products.Where(prod => prod.ProductKindId == 1 /*Тетради*/ ).ToList();
			}

			var productModels = _mapper.Map<IEnumerable<ProductModel>>(products).ToList();

			if (orderId != null)
			{
				var productsInOrder = _context.OptOrderproduct.Where(t => t.OrderId == orderId);
				await productsInOrder.ForEachAsync(t =>
					productModels.First(product => product.ProductId == t.ProductId).Amount = t.Amount);
			}
			else
			{
				productModels = productModels.Where(prod => !prod.IsArchived).ToList();
			}
			var seriesIds = products.Select(t => t.ProductSeriesId).Distinct();
			var series = await _context.OptProductseries
				.Where(t => seriesIds.Contains(t.ProductSeriesId))
				.OrderBy(t => t.ProductKindId).ThenBy(t => t.Order).ToListAsync();
			series.Add(new OptProductseries { ProductSeriesId = 0, Name = "" });

			var result = new Dictionary<OptProductseries, List<ProductModel>>();
			foreach (var item in series)
			{
				var values = productModels.Where(t => (t.ProductSeriesId ?? 0) == item.ProductSeriesId).OrderBy(product => (product.DisplayOrder ?? 0)).ToList();
				if (values.Any())
				{
					values.ForEach(p =>
					{
						var price = prices.FirstOrDefault(pr => pr.ProductId == p.ProductId);
						p.Price = price?.Price;
						if (p.ShowPreviousPrice && price != null)
						{
							var previousPrice = _priceRepository.GetPreviousPrice(p.ProductId, price.DateStart.Value
								);
							p.PreviousPrice = previousPrice?.Price;
						}
					});

					result.Add(item, values);
				}
			}

			return result;
		}

		public async Task<IEnumerable<ProductModel>> GetProductPlainListModel()
		{
			var raw = await _productRepository.GetProducts().OrderBy(product => product.ProductSeries.Order).ThenBy(product => product.DisplayOrder).ToListAsync();
			var list = _mapper.Map<IEnumerable<ProductModel>>(raw);
			return list;

		}

		public async Task<IEnumerable<OptProductseries>> GetProductSeries()
		{
			return await _context.OptProductseries.Where(series => series.ProductKindId == Const.NoteBookProductKind).ToListAsync();
		}

		public async Task<IEnumerable<OptCountry>> GetCountries()
		{
			return await _context.OptCountry.ToListAsync();
		}

		public ProductModel GetProductWithPrices(int productId)
		{
			var raw = _context.OptProduct
					.Include(product => product.Country)
					.Include(product => product.ProductSeries)
					.Include(product => product.ProductPrices)
					.FirstOrDefault(product => product.ProductId == productId);
			var model = _mapper.Map<ProductModel>(raw);
			return model;
		}

		public async Task AddKitProductAsync(KitProductModel model)
		{
			var kitProduct = new OptKitproduct
			{
				ProductId = model.ProductId,
				MainProductId = model.MainProductId,

			};
			await _productRepository.AddKitProductAsync(kitProduct);
		}

		public async Task<ProductListModel> GetProductListModel(bool onlyWithPrices, ApplicationUser user, bool dontShowDisabled = true)
		{
			ProductListModel model = await GetProducts(onlyWithPrices, user: user, dontShowDisabled: dontShowDisabled);

			var eventModel = _eventManager.GetEventByDate();
			if (eventModel != null)
				model.ActiveEvent = eventModel;
			return model;
		}

		public async Task<ProductListModel> GetProducts(bool onlyWithPrices = true, Guid? orderId = null, ApplicationUser user = null, bool dontShowDisabled = true)
		{
			var model = new ProductListModel();

			SetMinOrderSize(model, user.ClientType, orderId);

			model.ProductGroups = await GetAsync(onlyWithPrices, orderId, dontShowDisabled);

			var clients = _context.OptUser
				.Where(t => t.IsClient && t.EmailConfirmed && (t.ClientType == (int)ClientType.Shop || t.ClientType == (int)ClientType.JointPurchase))
				.OrderBy(t => t.LastName)
				.ThenBy(t => t.FirstName)
				.Select(t => new { t.Id, Name = t.LastName + " " + t.FirstName + "(" + t.Email + ")" });

			model.Clients = new SelectList(clients, "Id", "Name");
			return model;
		}

		public async Task CreateAsync(ProductModel model)
		{
			CheckUniqueArticleNumber(null, model.ArticleNumber);
			model.Weight = Convert.ToSingle(Math.Round(model.Weight, 1));
			var optProduct = new OptProduct
			{
				Name = model.Name,
				ProductKindId = model.ProductKindId,
				IsKit = model.IsKit,
				Enabled = model.Enabled,
				ShowPreviousPrice = model.ShowPreviousPrice,
				ProductSeriesId = model.ProductSeriesId,
				Weight = model.Weight,
				ISBN = model.ISBN,
				DisplayOrder = model.DisplayOrder,
				RegionalName = model.RegionalName,
				CountryId = model.CountryId,
				ProductId = model.ProductId, //for test purposes should stay 
				Semiproducts = _mapper.Map<ICollection<OptSemiproduct>>(model.Semiproducts)//for test purposes should stay 
			};

			await _productRepository.CreateAsync(optProduct);
		}

		private void CheckUniqueArticleNumber(int? productId, string articleNumber)
		{
			if (!_productRepository.IsUniqueArticleNumber(productId, articleNumber))
			{
				throw new UserException("Невозмоно сохрнить информцию. Не уникльный артикул");
			}
		}

		public IEnumerable<KitProductModel> GetKitProducts(int productId)
		{
			var list = _context.OptKitproduct?
					  .Where(kitP => kitP.MainProductId == productId)
					  .Include(kitP => kitP.Product)
						.ThenInclude(prod => prod.Country)
					  .OrderBy(kitP => kitP.OrderNum);
			var kitProducts = new List<KitProductModel>();
			foreach (var kitProduct in list)
			{
				var product = _mapper.Map<ProductModel>(kitProduct.Product);
				kitProducts.Add(new KitProductModel
				{
					KitProductId = kitProduct.KitProductId,
					ProductId = kitProduct.ProductId,
					MainProductId = kitProduct.MainProductId,
					ProductName = product.DisplayName,
					ProductEnabled = product.Enabled
				});
			}
			return kitProducts;
		}

		public IEnumerable<ProductModel> GetNotebooksExcludeExistings(IEnumerable<int> existingItems, int? productSeriesId)
		{
			var rawList = _context.OptProduct
				.Include(prod => prod.Country)
				.Where(prod => prod.ProductKindId == 1 /*Тетради*/ && !prod.IsKit
				&& !existingItems.Contains(prod.ProductId)
				&& prod.ProductSeriesId == productSeriesId);
			return _mapper.Map<IEnumerable<ProductModel>>(rawList);
		}

		public IEnumerable<int> GetExistingItems(int productId)
		{
			return _context.OptKitproduct.Where(kit => kit.MainProductId == productId)?.Select(kit => kit.ProductId).ToList() ?? Enumerable.Empty<int>();
		}

		public async Task Edit(ProductModel model)
		{
			CheckUniqueArticleNumber(model.ProductId, model.ArticleNumber);

			model.Weight = Convert.ToSingle(Math.Round(model.Weight, 1));
			var optProduct = _context.OptProduct.First(t => t.ProductId == model.ProductId);
			optProduct.Name = model.Name;
			optProduct.ProductKindId = model.ProductKindId;
			optProduct.IsKit = model.IsKit;
			optProduct.Enabled = model.Enabled;
			optProduct.ProductSeriesId = model.ProductSeriesId;
			optProduct.Weight = model.Weight;
			optProduct.ISBN = model.ISBN;
			optProduct.DisplayOrder = model.DisplayOrder;
			optProduct.RegionalName = model.RegionalName;
			optProduct.CountryId = model.CountryId;
			optProduct.ArticleNumber = model.ArticleNumber;
			optProduct.ShowPreviousPrice = model.ShowPreviousPrice;
			await _context.SaveChangesAsync();
		}

		public async Task Delete(int? id)
		{
			var optProduct = await _context.OptProduct.FirstOrDefaultAsync(m => m.ProductId == id);
			if (optProduct == null)
				throw new UserException($"Товар с id={id} не найден");
			_context.Remove(optProduct);
			await _context.SaveChangesAsync();
		}

		public void SetMinOrderSize(ProductListModel model, int clientType, Guid? orderId = null)
		{
			if (model == null)
				return;
			IQueryable<OptOrderSizeDiscount> orderSizeDiscounts = _context.OptOrderSizeDiscount
				.Where(oSize => oSize.From >= 0 && oSize.ClientType == clientType);

			var date = DateTime.Now;
			if (orderId != null)
			{
				date = _context.OptOrder.Find(orderId.Value).OrderDate;
			}

			orderSizeDiscounts = orderSizeDiscounts.Where(oSize =>
				oSize.DateStart <= date && date <= oSize.DateEnd ||
				oSize.DateStart <= date && oSize.DateEnd == null);

			model.OrderSizeDiscounts = orderSizeDiscounts;
		}

		public ProductModel GetDetails(int? productId)
		{
			if (productId == null)
			{
				return null;
			}

			var optProduct = _productRepository.GetDetails(productId.Value);

			if (optProduct == null)
				throw new UserException("Невозможно найти указанный товар");
			optProduct.Weight = Convert.ToSingle(Math.Round(optProduct.Weight, 1));
			var model = _mapper.Map<ProductModel>(optProduct);
			return model;
		}

		public async Task DeleteKitProduct(int id)
		{
			var kitProduct = await _context.OptKitproduct.FindAsync(id);
			if (kitProduct != null)
				_context.Remove(kitProduct);
			await _context.SaveChangesAsync();
		}

		public async Task ParsePriceFile(IFormFile file)
		{
			var list = new List<OptProductprice>();
			var products = _productRepository.GetNotebooks();
			using (var memoryStream = new MemoryStream())
			{
				await file.CopyToAsync(memoryStream).ConfigureAwait(false);

				using (var package = new ExcelPackage(memoryStream))
				{
					var worksheet = package.Workbook.Worksheets[0];

					list.AddRange(ReadSheet(products, worksheet));

					await _priceRepository.UpdateRangeAsync(list);
				}
			}
		}

		private IEnumerable<OptProductprice> ReadSheet(IOrderedQueryable<OptProduct> products, ExcelWorksheet worksheet)
		{
			var list = new List<OptProductprice>();
			for (var i = 2; i <= worksheet.Dimension.Rows; i++)
			{
				var isbn = worksheet.Cells[i, 1].Value?.ToString();
				if (string.IsNullOrWhiteSpace(isbn))
				{
					continue;
				}
				var productId = products.FirstOrDefault(prod =>
					!string.IsNullOrWhiteSpace(prod.ISBN) && prod.ISBN.Equals(isbn))
					?.ProductId;

				if (!productId.HasValue)
				{
					throw new UserException($"Не найдена тетрадь с isbn {isbn}");
				}
				var priceCellText = worksheet.Cells[i, 4].Value?.ToString();
				if (string.IsNullOrWhiteSpace(priceCellText)
					|| !double.TryParse(priceCellText, out var price)
					|| price <= 0
					)
				{
					throw new UserException($"Для isbn {isbn} не задана цена");
				}

				var product = new OptProductprice
				{
					ProductId = productId.Value,
					Price = price
				};
				list.Add(product);
			}
			return list;
		}

		public OptKitproduct GetKitProduct(int id)
		{
			return _context.OptKitproduct.FirstOrDefault(kitProduct =>
				 kitProduct.MainProductId == id);
		}

		public IEnumerable<ProductModel> GetProductsInKit(OptProduct mainProduct)
		{
			var products = _productRepository.GetKitProducts(mainProduct.ProductId).ToList();
			if (products == null || !products.Any() && mainProduct.IsKit)
				throw new UserException("Комплект не включает в себя ни одной тетради");
			if (mainProduct.ProductId == Const.CalendarId)
			{
				var gluePad = _productRepository.GetDetails(Const.GluePadProductId);
				products.Add(gluePad);
			}

			var productModels = _mapper.Map<IEnumerable<ProductModel>>(products);
			return productModels;
		}

		public async Task<List<ProductModel>> GetProductsByType(string typeParam)
		{
			List<OptProduct> products = null;
			var rawList = _context.OptProduct
				.Include(product => product.Country)
				.Include(product => product.ProductSeries)
				.Where(product => true)
				.OrderBy(product => product.ProductSeries.Order)
					.ThenBy(product => product.DisplayOrder);
			switch (typeParam)
			{
				case "onlyKit":
					products = await rawList.Where(x => x.IsKit == true && x.ProductKindId == 1).ToListAsync();
					break;
				case "withoutKit":
					products = await rawList.Where(x => x.IsKit != true && x.ProductKindId == 1).ToListAsync();
					break;
				case "all":
					products = await rawList.Where(x => x.ProductKindId == 1).ToListAsync();
					break;
			}
			var result = _mapper.Map<List<ProductModel>>(products);
			return result;
		}

		public async Task<IEnumerable<OptProductkind>> GetProductKinds()
		{
			return await _context.OptProductkind.ToListAsync();
		} 
	}
}
