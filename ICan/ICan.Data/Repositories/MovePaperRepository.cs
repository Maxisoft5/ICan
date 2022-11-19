using ICan.Common.Domain;
using ICan.Common.Repositories;
using ICan.Data.Context;
using ICan.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Data.Repostories
{
	public class MovePaperRepository : BaseRepository, IMovePaperRepository
	{
		public MovePaperRepository(ApplicationDbContext context) : base(context) { }

		public async Task<int> AddAsync(OptMovePaper movePaper)
		{
			await _context.AddAsync(movePaper);
			await _context.SaveChangesAsync();
			return movePaper.MovePaperId;
		}

		public async Task DeleteAsync(int id)
		{
			var movePaperRow = await _context.OptMovePaper.FirstOrDefaultAsync(x => x.MovePaperId == id);
			
			if(movePaperRow == null)
				throw new Exception($"Не найдена запись с указанным id = {id}");

			_context.Remove(movePaperRow);
			await _context.SaveChangesAsync();
		}

		public async Task<IEnumerable<OptMovePaper>> GetAsync()
		{
			var res = await _context.OptMovePaper
									.Include(x => x.Paper)
									.Include(x => x.SenderWarehouse)
									.Include(x => x.ReceiverWarehouse).ToListAsync();

			return res;
		}

        public async Task<OptMovePaper> GetByPrintOrderPaperId(int printOrderPaperId)
        {
			return await _context.OptMovePaper.FirstOrDefaultAsync(x => x.PrintOrderPaperId == printOrderPaperId);
        }
    }
}
