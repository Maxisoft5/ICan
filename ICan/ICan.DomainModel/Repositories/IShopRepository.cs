using ICan.Common.Domain;
using System.Linq;

namespace ICan.Common.Repositories
{
	public interface IShopRepository
	{
		IOrderedQueryable<OptShop> GetClientShops(string clientId);
	}
}