using AutoMapper;
using ICan.Common.Domain;
using ICan.Common.Jobs.OzonPrice;
using ICan.Common.Models.Enums;
using ICan.Common.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class OzonApiManager : BaseManager
	{
		private ISiteRepository _siteRepository;
		private IProductRepository _productRepository;

		public OzonApiManager(IMapper mapper,
			ILogger<BaseManager> logger,
			ISiteRepository siteRepository,
			IProductRepository productRepository) : base(mapper, logger)
		{
			_siteRepository = siteRepository;
			_productRepository = productRepository;
		}

		public async Task UploadRangeAsync(List<OzonItem> items)
		{
			var products = _productRepository.GetNotebooks();
			foreach (var ozonItem in items)
			{
				if (string.IsNullOrWhiteSpace(ozonItem.OfferISBN))
				{
					_logger.LogWarning($"[OzonApiPrice] ozon product has no isbn {ozonItem.ProductId}");
					continue;
				}

				if (!decimal.TryParse(ozonItem.Price.MarketingPrice?.Replace(".", ","), out var price))
				{
					_logger.LogWarning($"[OzonApiPrice] ozon product ${ozonItem.OfferISBN} has issues with price");
					continue;
				}
				var isbn = ozonItem.OfferISBN.Replace("-", "");
				var product = products.FirstOrDefault(notebook =>
					(!string.IsNullOrWhiteSpace(notebook.ISBN) &&
					notebook.ISBN.Equals(isbn)) || (!string.IsNullOrWhiteSpace(notebook.ArticleNumber) &&
					notebook.ArticleNumber.Equals(isbn)) );

				if (product == null)
				{
					_logger.LogWarning($"[OzonApiPrice] product with isbn {ozonItem.OfferISBN} which came from ozon,is not found");
					continue;
				}
				await _siteRepository.UpdatePriceAsync((int)Marketplace.Ozon, product.ProductId, price);
			}
		}

	}
}