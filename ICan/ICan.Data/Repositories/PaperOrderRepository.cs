using ICan.Common.Domain;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ICan.Data.Repositories
{
    public class PaperOrderRepository : BaseRepository, IPaperOrderRepository
    {
        public PaperOrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public IOrderedQueryable<OptPaperOrder> Get()
        {
            var list = _context.OptPaperOrder
                            .Include(party => party.RecieverCounterParty)
                            .Include(party => party.SupplierCounterParty)
                            .Include(party => party.Format)
                            .Include(party => party.Paper)
                            .Include(party => party.PrintOrderPapers)
                            .OrderByDescending(party => party.OrderDate);
            return list;
        }

    }
}
