using ICan.Common.Domain;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface ISpringOrderIncomingRepository
	{
		Task<int> Add(OptSpringOrderIncoming orderIncoming);
		Task Delete(int incomingId);
	}
}
