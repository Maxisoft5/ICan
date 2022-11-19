using ICan.Common.Domain;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICan.Data.Repositories
{
	public class CampaingRepository : BaseRepository, ICampaignRepository
	{
		public CampaingRepository(ApplicationDbContext context): base(context)
		{

		}

		public async Task<int> Add(OptCampaign campaing)
		{
			await _context.AddAsync(campaing);
			await _context.SaveChangesAsync();
			return campaing.CampaignId;
		}

		public async Task Delete(int id)
		{
			var campaing = await GetById(id);
			_context.Remove(campaing);
			await _context.SaveChangesAsync();
		}

		public IEnumerable<OptCampaign> Get()
		{
			return _context.OptCampaign;
		}

		public async Task<OptCampaign> GetById(int id)
		{
			return await _context.OptCampaign.FirstOrDefaultAsync(x => x.CampaignId == id);
		}

		public async Task Update(OptCampaign optCampaign)
		{
			_context.Update(optCampaign);
			await _context.SaveChangesAsync();
		}
	}
}
