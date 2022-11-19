using ICan.Common.Domain;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Test.Fakes
{
	public class PaperOrderRepository :  IPaperOrderRepository
	{
		public readonly List<OptPaperOrder> Entries = new List<OptPaperOrder>();

		public IOrderedQueryable<OptPaperOrder> Get()
		{
			return Entries.AsQueryable().OrderBy(paperO => paperO.OrderDate);
		}

        public async Task<OptPaperOrder> GetPaperOrderByPaperId(int paperId)
        {
			return await Entries.AsQueryable()
				.OrderByDescending(x => x.OrderDate)
				.FirstOrDefaultAsync(x => x.PaperId == paperId);
		}
    }
}
