using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Data.Repositories
{
	public class SiteRepository : BaseRepository, ISiteRepository
	{
		public SiteRepository(ApplicationDbContext context) : base(context)
		{
		}

		public async Task AddMarketPlaceInfoAsync(IEnumerable<OptMarketplaceProduct> list)
		{
			var existingList = await _context.OptMarketplaceProduct
				.Include(item => item.Urls)
				.ToListAsync();
			foreach (var item in list)
			{
				var existing = existingList
					.FirstOrDefault(exst => exst.ProductId == item.ProductId
					&& exst.MarketplaceId == item.MarketplaceId);
				if (existing == null)
				{
					existing = new OptMarketplaceProduct
					{
						ProductId = item.ProductId,
						MarketplaceId = item.MarketplaceId,
						Price = item.Price,
						Raiting = item.Raiting,
						ReviewsAmount = item.ReviewsAmount,
					};
					_context.Add(existing);
				}
				else
				{
					existing.Price = item.Price;
					existing.Raiting = item.Raiting;

					existing.ReviewsAmount = item.ReviewsAmount;
					if (item.Urls != null && item.Urls.Any())
					{
						_context.RemoveRange(existing.Urls);
						existing.Urls = item.Urls;
					}

				}
				await _context.SaveChangesAsync();
			}
		}

		public async Task AddMarketPlaceProductAsync(OptMarketplaceProduct marketProduct)
		{
			await _context.OptMarketplaceProduct.AddAsync(marketProduct);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateMarketPlaceProductAsync(OptMarketplaceProduct marketProduct)
		{
			var existing = marketProduct.Urls.Select(url => url.MarketplaceProductUrlId);
			var urlsToDelete = _context.OptMarketplaceProductUrl
				.Where(url => !existing.Contains(url.MarketplaceProductUrlId) && url.MarketplaceProductId == marketProduct.MarketplaceProductId);
			_context.RemoveRange(urlsToDelete);
			_context.Update(marketProduct);
			await _context.SaveChangesAsync();
		}



		public async Task UpdatePriceAsync(int marketPlaceId, int productId, decimal price)
		{
			var existing = await _context.OptMarketplaceProduct.FirstOrDefaultAsync(db => db.ProductId == productId && db.MarketplaceId == marketPlaceId);
			if (existing == null)
			{
				existing = new OptMarketplaceProduct
				{
					ProductId = productId,
					MarketplaceId = marketPlaceId,
					Price = price,
					Raiting = Const.DefautRating
				};
				_context.Add(existing);
			}
			else
			{
				existing.Price = price;
			}
			await _context.SaveChangesAsync();
		}


		public async Task UpdateAsync(int marketPlaceId, int productId, decimal? price, int? reviews, float? rating)
		{
			var existing = await _context.OptMarketplaceProduct						
				.FirstOrDefaultAsync(db => db.ProductId == productId && db.MarketplaceId == marketPlaceId);
			if (existing == null)
			{
				existing = new OptMarketplaceProduct
				{
					ProductId = productId,
					MarketplaceId = marketPlaceId,
					Price = price,
					Raiting = rating,
					ReviewsAmount = reviews
				};
				_context.Add(existing);
			}
			else
			{
				existing.Price = price;
				existing.Raiting = rating;
				existing.ReviewsAmount = reviews;
			}
			await _context.SaveChangesAsync(); 
		}


		public async Task DeleteTag(int productTagId)
		{
			var raw = await _context.OptProductTag.FirstOrDefaultAsync(pTag => pTag.ProductTagId == productTagId);
			if (raw == null)
				return;

			_context.Remove(raw);
			await _context.SaveChangesAsync();
		}

		public int? FindMarketPlaceByName(string name)
		{
			var marketPlace = _context.OptMarketplace.FirstOrDefault(mPlace =>
				!string.IsNullOrWhiteSpace(mPlace.Name) && mPlace.Name.Equals(name));
			return marketPlace?.MarketplaceId;
		}

		public IEnumerable<OptProduct> GetByFilterAndTag(int siteId, ClientFilter clientFilter)
		{
			var filterCode = clientFilter.Filter ?? 1;

			var query = _context.OptSiteFilterProduct
				.Include(filterProduct => filterProduct.SiteFilter)
				.Where(filterProduct =>

				 clientFilter.CategoryFilter.HasValue && filterProduct.SiteFilterId == clientFilter.CategoryFilter.Value
				||

				 !clientFilter.CategoryFilter.HasValue && filterProduct.SiteFilter.FilterCode == filterCode && filterProduct.SiteFilter.SiteId == siteId)

				.Include(filterProduct => filterProduct.Product.ProductImages)
				.Include(filterProduct => filterProduct.Product)
					.ThenInclude(prod => prod.ProductTags)
						.ThenInclude(prodTag => prodTag.Tag)
				.Include(filterProduct => filterProduct.Product.MarketplaceProducts)
					.ThenInclude(marketPlaceProduct => marketPlaceProduct.Marketplace)
				.Where(filterProduct => (!clientFilter.Tag.HasValue || filterProduct.Product.ProductTags.Any(prodTag => prodTag.TagId == clientFilter.Tag.Value)));
			if (!string.IsNullOrWhiteSpace(clientFilter.NotebookLang))
			{
				if (clientFilter.NotebookLang.Equals("en", System.StringComparison.InvariantCultureIgnoreCase))
				{
					query = query.Where(filterProduct => filterProduct.Product.CountryId.HasValue);
				}
				else
				{
					query = query.Where(filterProduct => !filterProduct.Product.CountryId.HasValue);
				}
			}
			else if (siteId == 1 && !clientFilter.CategoryFilter.HasValue)
			{
				query = query.Where(filterProduct => !filterProduct.Product.CountryId.HasValue);
			}

			var products = query.OrderBy(filterProduct => filterProduct.Order)
					  .ToList()
					  .Select(filterProduct => (filterProduct.Product));
			return products;
		}

		public OptMarketplaceProduct GetMarketPlaceProduct(int marketPlaceProductId)
		{
			var raw = _context.OptMarketplaceProduct
				.Include(mProduct => mProduct.Urls)
				.Include(mProduct => mProduct.Marketplace)
				.First(mProduct => mProduct.MarketplaceProductId == marketPlaceProductId);
			return raw;
		}

		public IEnumerable<OptMarketplace> GetAvailableMarketplaces(int productId)
		{
			var existing = _context.OptMarketplaceProduct
			   .Where(mProd => mProd.ProductId == productId)
			   .Select(mProd => mProd.MarketplaceId);
			var marketPlaces = _context.OptMarketplace
				.Where(market => !existing.Contains(market.MarketplaceId));
			return marketPlaces;
		}

		public async Task DeleteMarketplaceProductAsync(int marketPlaceProductId)
		{
			var raw = await _context.OptMarketplaceProduct.FirstOrDefaultAsync(mProduct => mProduct.MarketplaceProductId == marketPlaceProductId);
			if (raw == null)
				return;

			_context.Remove(raw);
			await _context.SaveChangesAsync();
		}

		public List<OptMarketplaceProduct> GetMarketplaceProducts(int marketPlaceId)
		{
			var products = _context.OptMarketplaceProduct
				.Include(prod => prod.Urls)				
				.Include(prod => prod.Product)
				.Where(prod => prod.MarketplaceId == marketPlaceId);
			return products.ToList();
		}

		public string GetLocale(int siteId)
		{
			var site = _context.OptSite.Find(siteId);
			return site.Locale;
		}

		public async Task<OptProduct> GetById(int id)
		{
			return await _context.OptProduct
			.Include(prod => prod.ProductSeries)
			.Include(prod => prod.ProductImages)
			.Include(prod => prod.ProductTags)
				.ThenInclude(prodTag => prodTag.Tag)
			.Include(prod => prod.MarketplaceProducts)
				.ThenInclude(marketProduct => marketProduct.Marketplace)
			.Include(prod => prod.MarketplaceProducts)
			.ThenInclude(marketProduct => marketProduct.Urls)
			.FirstOrDefaultAsync(prod => prod.ProductId == id);
		}

		public IQueryable<int> GetProductTagIds(int productId)
		{
			var productTagIds = _context.OptProductTag
				.Where(productTag => productTag.ProductId == productId).Select(productTag => productTag.TagId);
			return productTagIds;
		}
	}
}
