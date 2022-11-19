using ICan.Common.Domain;
using ICan.Common.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Test.Fakes
{
	public class FakePriceRepository : IPriceRepository
	{
		private IProductRepository _productRepository { get; set; }

		public FakePriceRepository(IProductRepository productRepository)
		{
			_productRepository = productRepository;
		}

	 
		public IQueryable<OptProductprice> GetPrices(DateTime now)
		{
			 return _productRepository.GetNotebooks()
				.SelectMany(prod => prod.ProductPrices)
				.Where(price => ((price.DateEnd.HasValue && price.DateStart.HasValue &&
				now >= price.DateStart.Value &&
				now < price.DateEnd.Value)
				|| (!price.DateEnd.HasValue && price.DateStart.HasValue && now >= price.DateStart)
				|| (!price.DateEnd.HasValue && !price.DateStart.HasValue)))
				.OrderBy(q => q.ProductId);
		}

		public async Task AddAsync(OptProductprice price)
		{
			var product = _productRepository.GetDetails(price.ProductId);
			product.ProductPrices.Add(price);
		}

		public async Task<OptProductprice> GetPriceForProductAsync(int productId, DateTime dateStart)
		{
			var product = _productRepository.GetDetails(productId);
			return product.ProductPrices.FirstOrDefault(price =>  price.DateEnd == null && price.DateStart <= dateStart);
		} 

		public async Task<OptProductprice> GetAsync(int productPriceId)
		{
			return _productRepository.GetNotebooks().SelectMany(prod => prod.ProductPrices)
				.FirstOrDefault(price => price.ProductPriceId == productPriceId);
		}

		public async Task UpdateAsync(OptProductprice oldPrice)
		{
			var price = await GetAsync(oldPrice.ProductPriceId);
			price.DateEnd = oldPrice.DateEnd;
			price.DateStart = oldPrice.DateStart;
			price.Price = oldPrice.Price;
		}

		public Task RemoveAsync(int productPriceId)
		{
			throw new NotImplementedException();
		}

		public Task UpdateRangeAsync(IEnumerable<OptProductprice> list)
		{
			throw new NotImplementedException();
		}

		public OptProductprice GetPreviousPrice(int productId, DateTime priceDate)
		{
			throw new NotImplementedException();
		}
	}
}
