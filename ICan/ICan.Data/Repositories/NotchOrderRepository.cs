using ICan.Common.Domain;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ICan.Data.Repositories
{
	public class NotchOrderRepository : BaseRepository, INotchOrderRepository
	{
		public NotchOrderRepository(ApplicationDbContext context): base(context)
		{
			
		}

		public async Task Add(OptNotchOrder notchOrder)
		{
			await _context.AddAsync(notchOrder);
			await _context.SaveChangesAsync();
		}

		public async Task<OptNotchOrderIncomingItem> GetIncomingItemById(int incomingItemId)
		{
			return await _context.OptNotchOrderIncomingItem.FirstOrDefaultAsync(x => x.NotchOrderIncomingItemId == incomingItemId);
		}

		public async Task UpdateIncomingItem(OptNotchOrderIncomingItem item)
		{
			_context.Update(item);
			await _context.SaveChangesAsync();
		}
	}
}
