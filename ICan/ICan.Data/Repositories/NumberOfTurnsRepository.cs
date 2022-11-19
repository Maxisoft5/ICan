using ICan.Common.Domain;
using ICan.Common.Repositories;
using ICan.Data.Context;
using ICan.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICan.Data.Repostories
{
	public class NumberOfTurnsRepository: BaseRepository, INumberOfTurnsRepository
	{
		public NumberOfTurnsRepository(ApplicationDbContext context): base(context)
		{

		}

		public async Task<IEnumerable<OptNumberOfTurns>> Get()
		{
			return await _context.OptNumberOfTurns.ToListAsync();
		}
	}
}
