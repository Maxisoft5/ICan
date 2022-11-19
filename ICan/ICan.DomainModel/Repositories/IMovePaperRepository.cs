using ICan.Common.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface IMovePaperRepository
	{
		Task<IEnumerable<OptMovePaper>> GetAsync();
		Task<OptMovePaper> GetByPrintOrderPaperId(int printOrderPaperId);
		Task<int> AddAsync(OptMovePaper movePaper);
		Task DeleteAsync(int id);
	}
}
