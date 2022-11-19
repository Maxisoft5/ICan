using ICan.Common.Domain;
using ICan.Common.Models.Exceptions;
using ICan.Common.Repositories;
using ICan.Data.Context;
using ICan.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICan.Data.Repostories
{
	public class SpringRepository : BaseRepository, ISpringRepository
	{
		public SpringRepository(ApplicationDbContext context) : base(context)
		{
		}

		public async Task<int> Create(OptSpring optSpring)
		{
			await _context.AddAsync(optSpring);
			await _context.SaveChangesAsync();
			return optSpring.SpringId;
		}

		public async Task Delete(int id)
		{
			var spring = await _context.OptSpring.FirstOrDefaultAsync(x => x.SpringId == id);
			
			if (spring == null)
				throw new UserException("Указанная пружина не найдена");

			_context.Remove(spring);
			await _context.SaveChangesAsync();
		}

		public async Task<IEnumerable<OptSpring>> Get()
		{
			return await _context.OptSpring.Include(x => x.NumberOfTurns).ToListAsync();
		}

		public async Task<OptSpring> GetById(int id)
		{
			return await _context.OptSpring.Include(x => x.NumberOfTurns).FirstOrDefaultAsync(x => x.SpringId == id);
		}

		public async Task Update(OptSpring mappedModel)
		{
			_context.OptSpring.Update(mappedModel);
			await _context.SaveChangesAsync();
		}
	}
}
