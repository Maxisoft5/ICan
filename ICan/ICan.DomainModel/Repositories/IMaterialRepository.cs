using ICan.Common.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface IMaterialRepository
	{
		IEnumerable<MaterialImage> Get(bool onlyActive, int? materialId = null);
		Task AddAsync(OptMaterial raw);
		OptMaterial GetWithoutImages(int materialId);
		Task UpdateAsync(OptMaterial raw);
		Task Delete(OptMaterial material);
		MaterialImage GetByFileName(string fileName);
	}
}