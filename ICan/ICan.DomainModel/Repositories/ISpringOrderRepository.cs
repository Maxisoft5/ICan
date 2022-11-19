using ICan.Common.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface ISpringOrderRepository
	{
		IQueryable<OptSpringOrder> GetSpringOrders();
		Task<int> Add(OptSpringOrder springOrder);
		Task<OptSpringOrder> GetById(int id);
		Task Update(OptSpringOrder springOrder);
		Task Delete(int id);
	}
}
