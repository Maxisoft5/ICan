using ICan.Common.Domain;
using ICan.Common.Models.Opt;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface ISiteRepository
	{
		IEnumerable<OptProduct> GetByFilterAndTag(int siteId, ClientFilter siteFilter);
		int? FindMarketPlaceByName(string name);
		Task AddMarketPlaceInfoAsync(IEnumerable<OptMarketplaceProduct> list);
		Task AddMarketPlaceProductAsync(OptMarketplaceProduct marketProduct);
		List<OptMarketplaceProduct> GetMarketplaceProducts(int marketPlaceId);

		Task UpdateMarketPlaceProductAsync(OptMarketplaceProduct raw);
		Task UpdatePriceAsync(int marketPlaceId, int productId, decimal price);
		Task UpdateAsync(int marketPlaceId, int productId, decimal? price, int? reviews, float? rating);
		Task DeleteTag(int productTagId);
		OptMarketplaceProduct GetMarketPlaceProduct(int marketPlaceProductId);
		IEnumerable<OptMarketplace> GetAvailableMarketplaces(int productId);
		Task DeleteMarketplaceProductAsync(int marketPlaceProductId);
		string GetLocale(int siteId);
		Task<OptProduct> GetById(int id); 
		IQueryable<int> GetProductTagIds(int productId);
	}
}