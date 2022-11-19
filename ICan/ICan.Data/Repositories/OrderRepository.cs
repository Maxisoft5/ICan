using ICan.Common.Repositories;
using ICan.Data.Context;
using System;
using System.Threading.Tasks;

namespace ICan.Data.Repositories
{
	public class OrderRepository : BaseRepository, IOrderRepository
	{
		public OrderRepository(ApplicationDbContext context) : base(context)
		{
		}
 
		public async Task<int> GetShortOrderId(Guid guid)
		{
			var order = await _context.OptOrder.FindAsync(guid);
			return order.ShortOrderId;
		}
	}
}
