using ICan.Common.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface IProductRepository
	{
		/// <summary>
		/// Returns all products which are Notebooks order by Series order,
		/// then by  Product DispayOrder 
		/// </summary>
		IOrderedQueryable<OptProduct> GetNotebooks();
		OptProduct GetDetails(int productId);
		OptProduct GetDetailsWithSemiproducts(int productId);
		Task CreateAsync(OptProduct optProduct);
		Task AddKitProductAsync(OptKitproduct kitProduct);
		IQueryable<OptProduct> GetKitProducts(int mainProductId);
		IQueryable<OptProduct> GetWithKitProducts(IEnumerable<int> productIds);
		bool IsUniqueArticleNumber(int? productId, string articleNumber);
		Task UpdateAsync(OptProduct raw);
		Task UpdateRangeAsync(IEnumerable<OptProduct> range);
		IQueryable<OptProduct> GetProducts();
	}
}