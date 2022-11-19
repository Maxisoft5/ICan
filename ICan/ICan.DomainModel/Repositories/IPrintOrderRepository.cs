using ICan.Common.Domain;
using ICan.Common.Models.Opt;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface IPrintOrderRepository
	{
		Task<OptPrintOrder> CreateAsync(PrintOrderModel model);
		Task DeleteAsync(int printOrderId);
		IEnumerable<OptPrintOrder> Get();
		Task<OptPrintOrder> GetByIdAsync(int id);
		Task<IEnumerable<OptPrintOrder>> GetByPaperIdAsync(int id);
		List<int> GetSemiproductdById(int id);
	}
}