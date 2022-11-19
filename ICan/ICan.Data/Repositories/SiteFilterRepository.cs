using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Data.Repositories
{
	public class SiteFilterRepository : BaseRepository, ISiteFilterRepository
	{
		public SiteFilterRepository(ApplicationDbContext context) : base(context)
		{
		}

		public IQueryable<OptSiteFilter> GetAll()
		{
			return _context.OptSiteFilter
				.Include(siteFilter => siteFilter.Site)
				.Include(siteFilter => siteFilter.Products)
					.ThenInclude(product => product.Product.Country);

		}


		public async Task UpdateAsync(OptProductprice oldPrice)
		{
			_context.Update(oldPrice);
			await _context.SaveChangesAsync();
		}

		public async Task<OptSiteFilter> GetAsync(int siteFilterId)
		{
			var filter = await GetAll().FirstOrDefaultAsync(siteFilter => siteFilter.SiteFilterId == siteFilterId);
			return filter;
		}

		public async Task DeleteFilterProductAsync(int siteFilterProductId)
		{
			var filterProduct = await GetFilterProductAsync(siteFilterProductId);
			if (filterProduct == null)
				return;
			_context.Remove(filterProduct);
			await _context.SaveChangesAsync();
		}

		public async Task<IOrderedEnumerable<OptProduct>> GetAvailableProductsAsync(int siteFilterId)
		{
			var existing = await _context.OptSiteFilterProduct
				.Where(siteFilterP => siteFilterP.SiteFilterId == siteFilterId)
				.Select(siteFilterP => siteFilterP.ProductId).ToListAsync();
			var products = _context.OptProduct
				.Include(prod => prod.ProductSeries)
				.Include(prod => prod.Country)
				.Where(prod => !existing.Contains(prod.ProductId) && prod.ProductKindId == (int)ProductKind.Notebook)
				.ToList()
				.OrderBy(prod => prod.ProductSeries.Order)
					.ThenBy(prod => prod.DisplayOrder);
			return products;
		}

		public async Task AddFilterProductAsync(OptSiteFilterProduct raw)
		{
			await _context.AddAsync(raw);
			await _context.SaveChangesAsync();
		}

		public async Task AddAsync(OptSiteFilter raw)
		{
			await _context.AddAsync(raw);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var filterProduct = await _context.OptSiteFilter.FirstOrDefaultAsync(fProd => fProd.SiteFilterId == id);
			if (filterProduct == null)
				return;
			_context.Remove(filterProduct);
			await _context.SaveChangesAsync();
		}

		public async Task<OptSiteFilterProduct> GetFilterProductAsync(int siteFilterProductId)
		{
			return await _context.OptSiteFilterProduct
				.Include(filter => filter.Product)
					.ThenInclude(filter => filter.Country)
				.FirstOrDefaultAsync(fProd => fProd.SiteFilterProductId == siteFilterProductId);
		}

		public  async Task UpdateFilterProductAsync(OptSiteFilterProduct raw)
		{
			var filterProduct = await GetFilterProductAsync(raw.SiteFilterProductId);
			filterProduct.Order = raw.Order;
			await _context.SaveChangesAsync();
		}
	}
}
