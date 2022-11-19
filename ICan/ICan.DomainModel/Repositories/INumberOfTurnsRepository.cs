using ICan.Common.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface INumberOfTurnsRepository
	{
		Task<IEnumerable<OptNumberOfTurns>> Get();
	}
}
