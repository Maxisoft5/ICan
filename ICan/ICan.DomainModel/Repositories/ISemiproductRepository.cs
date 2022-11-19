using ICan.Common.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface ISemiproductRepository
	{
		IQueryable<OptSemiproductType> GetSemiproductTypes();
		IQueryable<OptSemiproduct> Get();
		Task<OptSemiproduct> GetAsync(int id);
		IQueryable<OptSemiproductPaper> GetSemiproductPapers();
		IQueryable<OptFormat> GetFormat();
		IQueryable<OptPaper> GetPaper();
		IQueryable<OptSemiproduct> FindDuplicate(int semiproductId, int semiproductTypeId, int productId, string name = "");

		Task Add(OptSemiproduct raw);
		IQueryable<OptSemiproduct> GetSemiproductsWithTypes();
	}
}