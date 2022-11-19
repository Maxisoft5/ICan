using ICan.Common.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface ISiteFilterRepository
	{
		IQueryable<OptSiteFilter> GetAll();
		Task<OptSiteFilter> GetAsync(int siteFilterId);
		Task DeleteFilterProductAsync(int siteFilterProductId);
		Task<IOrderedEnumerable<OptProduct>> GetAvailableProductsAsync(int id);
		Task AddFilterProductAsync(OptSiteFilterProduct raw);
		Task DeleteAsync(int id);
		Task AddAsync(OptSiteFilter raw);
		Task<OptSiteFilterProduct> GetFilterProductAsync(int siteFilterProductId);
		Task UpdateFilterProductAsync(OptSiteFilterProduct raw);
	}
}