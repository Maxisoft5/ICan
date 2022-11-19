using ICan.Common.Domain;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface INotchOrderRepository
	{
		Task Add(OptNotchOrder notchOrder);
		Task<OptNotchOrderIncomingItem> GetIncomingItemById(int incomingItemId);
		Task UpdateIncomingItem(OptNotchOrderIncomingItem item);
	}
}
