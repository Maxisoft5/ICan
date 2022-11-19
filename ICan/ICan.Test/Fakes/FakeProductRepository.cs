using ICan.Common.Domain;
using ICan.Common.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Test.Fakes
{
	public class FakeProductRepository : IProductRepository
	{
		public readonly List<OptProduct> Products = new List<OptProduct>();

		public async Task AddKitProductAsync(OptKitproduct kitProduct)
		{
			var product = GetDetails(kitProduct.MainProductId);
			var subProduct = GetDetails(kitProduct.ProductId);
			kitProduct.Product = subProduct;
			if (product == null)
				return;
			product.KitProducts.Add(kitProduct);
		}

		public async Task CreateAsync(OptProduct optProduct)
		{
			Products.Add(optProduct);
		}

		public OptProduct GetDetails(int productId)
		{
			return Products.FirstOrDefault(product => product.ProductId == productId);
		}

		public OptProduct GetDetailsWithSemiproducts(int productId)
		{
			return Products.FirstOrDefault(product => product.ProductId == productId);
		}
		
		public IQueryable<OptProduct> GetProducts()
		{
			return Products.AsQueryable();
		}

		public IQueryable<OptProduct> GetKitProducts(int mainProductId)
		{
			/*
		   return _context
			   .OptKitproduct
			   .Where(x => x.MainProductId == mainProductId)
			   .Include(x => x.Product)
				   .ThenInclude(product => product.Country)
			   .Select(x => x.Product);
			 */
			var product = GetDetails(mainProductId);
			ICollection<OptKitproduct> result = product.KitProducts ?? new HashSet<OptKitproduct>();
			return result.Select(kitProduct => kitProduct.Product).AsQueryable();
		}

		public IOrderedQueryable<OptProduct> GetNotebooks()
		{
			return Products.AsQueryable().OrderBy(product => product.DisplayOrder);
		}

		public IQueryable<OptProduct> GetWithKitProducts(IEnumerable<int> productIds)
		{
			return Products.Where(product => productIds.Contains(product.ProductId))
			.AsQueryable();
		}

		public bool IsUniqueArticleNumber(int? productId, string articleNumber)
		{
			if (string.IsNullOrWhiteSpace(articleNumber))
				return true;
			articleNumber = articleNumber.ToLower().Trim();
			var otherProductsExist = Products.Any(prod =>
			!string.IsNullOrWhiteSpace(prod.ArticleNumber) && prod.ArticleNumber.Trim().ToLower()
			.Equals(articleNumber) && (!productId.HasValue || productId.Value != prod.ProductId));
			return !otherProductsExist;
		}

		public async Task UpdateAsync(OptProduct raw)
		{
			var product = Products.First(prod => prod.ProductId == raw.ProductId);
			Products.Remove(product);
			Products.Add(raw);
		}

		public async Task UpdateRangeAsync(IEnumerable<OptProduct> range)
		{
			throw new System.NotImplementedException();
		}
	}
}
