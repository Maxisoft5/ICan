using ICan.Common.Domain;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Data.Repositories
{
    public class PrintOrderPaperRepository : BaseRepository, IPrintOrderPaperRepository
    {
        public PrintOrderPaperRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<OptPrintOrderPaper> GetByIdAsync(int id)
        {
            return await _context.OptPrintOrderPaper.Include(x => x.PaperOrder)
                .ThenInclude(x => x.Paper).ThenInclude(x => x.TypeOfPaper)
                .Include(x => x.PrintOrder)
                .FirstOrDefaultAsync(x => x.PrintOrderPaperId == id);
        }

        public async Task<IEnumerable<OptPrintOrderPaper>> GetAllByPaperIdAsync(int paperId)
        {
            return await _context.OptPrintOrderPaper
                .Include(x => x.PrintOrder).Include(x => x.PaperOrder).ThenInclude(x => x.Paper)
                .Where(x => x.PaperOrder.Paper.PaperId == paperId && x.PrintOrder.IsArchived == false)
                .AsNoTracking().ToListAsync();
        }
    }
}
