using ICan.Common.Domain;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Data.Repositories
{
	public class ProductRepository : BaseRepository, IProductRepository
	{
		public ProductRepository(ApplicationDbContext context) : base(context)
		{
		}

		public async Task AddKitProductAsync(OptKitproduct kitProduct)
		{
			await _context.AddAsync(kitProduct);
			await _context.SaveChangesAsync();
		}

		public async Task CreateAsync(OptProduct optProduct)
		{
			await _context.AddAsync(optProduct);
			await _context.SaveChangesAsync();
		}

		public OptProduct GetDetails(int productId)
		{
			return _context.OptProduct
				.Include(t => t.Country)
				.Include(t => t.ProductKind)
				.FirstOrDefault(m => m.ProductId == productId);
		}

		public OptProduct GetDetailsWithSemiproducts(int productId)
		{
			return _context.OptProduct
				.Include(t => t.Country)
				.Include(t => t.ProductKind)
				.Include(t => t.Semiproducts)
					.ThenInclude(prod => prod.SemiproductType)
				.FirstOrDefault(m => m.ProductId == productId);
		}

		public IQueryable<OptProduct> GetKitProducts(int mainProductId)
		{
			return _context
				.OptKitproduct
				.Where(x => x.MainProductId == mainProductId)
				.Include(x => x.Product)
					.ThenInclude(product => product.Country)
				.Select(x => x.Product);
		}

		public IQueryable<OptProduct> GetProducts()
		{
			return _context.OptProduct
				.Include(product => product.KitProducts)
					.ThenInclude(kitProduct => kitProduct.Product)
						.ThenInclude(product => product.Country)
				.Include(product => product.ProductKind)
				.Include(product => product.Country)
				.Include(product => product.ProductSeries)
				.Where(product => product.ProductKindId == 1);
		}

		public IOrderedQueryable<OptProduct> GetNotebooks()
		{
			return _context.OptProduct
				.Include(product => product.Country)
				.Include(product => product.ProductSeries)
				.Where(product => product.ProductKindId == 1 /*Тетради*/)
				.OrderBy(product => product.ProductSeries.Order)
					.ThenBy(product => product.DisplayOrder);
		}

		public IQueryable<OptProduct> GetWithKitProducts(IEnumerable<int> productIds)
		{
			return _context.OptProduct
				.Include(product => product.KitProducts)
					.ThenInclude(kitProduct => kitProduct.Product)
				.Where(t => productIds.Contains(t.ProductId));
		}

		public bool IsUniqueArticleNumber(int? productId, string articleNumber)
		{
			if (string.IsNullOrWhiteSpace(articleNumber))
				return true;
			articleNumber = articleNumber.ToLower().Trim();
			var otherProductsExist = _context.OptProduct.Any(prod =>
			!string.IsNullOrWhiteSpace(prod.ArticleNumber) && prod.ArticleNumber.Trim().ToLower()
			.Equals(articleNumber) && (!productId.HasValue || productId.Value != prod.ProductId));
			return !otherProductsExist;
		}

		public async Task UpdateAsync(OptProduct raw)
		{
			_context.Update(raw);
			await _context.SaveChangesAsync();
		}
		
		public async Task UpdateRangeAsync(IEnumerable<OptProduct> range)
		{
			_context.UpdateRange(range);
			await _context.SaveChangesAsync();
		}
	}
}
