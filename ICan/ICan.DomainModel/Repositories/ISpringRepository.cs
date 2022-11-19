using ICan.Common.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface ISpringRepository
	{
		Task<IEnumerable<OptSpring>> Get();
		Task<int> Create(OptSpring optSpring);
		Task<OptSpring> GetById(int id);
		Task Update(OptSpring mappedModel);
		Task Delete(int id);
	}
}
