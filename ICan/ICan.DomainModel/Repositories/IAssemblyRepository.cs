using ICan.Common.Domain;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface IAssemblyRepository
	{
		Task<OptAssembly> GetAssembly(long assemblyId);
		Task Add(OptAssembly raw);
	}
}