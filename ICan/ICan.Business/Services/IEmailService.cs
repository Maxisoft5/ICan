using ICan.Common.Domain;
using ICan.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICan.Business.Services
{
	public interface IEmailService
	{
		Task<int> ExportContacts(IEnumerable<ApplicationUser> contacts);
		Task<string> PrepareCampaign(OptCampaign campaign);
		Task SendCampaign(string externalCampaignId);
		Task AddClient(ApplicationUser client);
		Task RemoveClient(ApplicationUser client);
		Task UpdateClient(ApplicationUser client);
	}
}
