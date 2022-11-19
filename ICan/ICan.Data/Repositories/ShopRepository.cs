using ICan.Common.Domain;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ICan.Data.Repositories
{
	public class ShopRepository : BaseRepository, IShopRepository
	{
		public ShopRepository(ApplicationDbContext context) : base(context)
		{
		}

		public IOrderedQueryable<OptShop> GetClientShops(string clientId)
		{
			var query = _context.OptApplicationUserShopRelation
				.Include(relation => relation.Shop)
				.Where(relation => relation.UserId == clientId)
				.Select(relation => relation.Shop)
				.OrderBy(shop => shop.Name);
			return query;
		}
	}
}
