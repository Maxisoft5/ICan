using ICan.Common.Domain;
using ICan.Common.Models.Exceptions;
using ICan.Common.Repositories;
using ICan.Data.Context;
using ICan.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ICan.Data.Repostories
{
	public class SpringOrderIncomingRepository : BaseRepository, ISpringOrderIncomingRepository
	{
		public SpringOrderIncomingRepository(ApplicationDbContext context) : base(context) { }
		
		public async Task<int> Add(OptSpringOrderIncoming orderIncoming)
		{
			await _context.AddAsync(orderIncoming);
			await _context.SaveChangesAsync();
			return orderIncoming.SpringOrderIncomingId;
		}

		public async Task Delete(int incomingId)
		{
			var incoming = await _context.OptSpringOrderincoming.FirstOrDefaultAsync(x => x.SpringOrderIncomingId == incomingId);
			
			if (incoming == null)
				throw new UserException("Указанный приход не найден");
			
			_context.Remove(incoming);
			await _context.SaveChangesAsync();
		}
	}
}
