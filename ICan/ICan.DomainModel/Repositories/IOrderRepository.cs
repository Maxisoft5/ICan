using System;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface IOrderRepository
	{
		Task<int> GetShortOrderId(Guid guid);
	}
}