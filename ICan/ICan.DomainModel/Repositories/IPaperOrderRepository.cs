using ICan.Common.Domain;
using System.Linq;

namespace ICan.Common.Repositories
{
    public interface IPaperOrderRepository
    {
        IOrderedQueryable<OptPaperOrder> Get();
    }
}