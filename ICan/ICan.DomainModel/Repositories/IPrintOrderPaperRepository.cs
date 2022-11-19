using ICan.Common.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
    public interface IPrintOrderPaperRepository
    {
        Task<OptPrintOrderPaper> GetByIdAsync(int id);
        Task<IEnumerable<OptPrintOrderPaper>> GetAllByPaperIdAsync(int paperId);

    }
}
