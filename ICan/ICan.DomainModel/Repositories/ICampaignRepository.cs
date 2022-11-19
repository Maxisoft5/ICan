using ICan.Common.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICan.Common.Repositories
{
	public interface ICampaignRepository
	{
		Task<int> Add(OptCampaign campaing);
		IEnumerable<OptCampaign> Get();
		Task<OptCampaign> GetById(int id);
		Task Update(OptCampaign optCampaing);
		Task Delete(int id);
	}
}
