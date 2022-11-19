using AutoMapper;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class CommonManager<TDb> : BaseManager where TDb : class
	{
		public CommonManager(IMapper mapper, ApplicationDbContext context, ILogger<BaseManager> logger) :
			base(mapper, context, logger)
		{
		}

		public async Task<IEnumerable<TDb>> GetAsync()
		{
			return await _context.Set<TDb>().ToListAsync();
		}

		public async Task<IEnumerable<TModel>> GetAsync<TModel>()
		{
			var rawList = await _context.Set<TDb>().ToListAsync();
			return _mapper.Map<IEnumerable<TModel>>(rawList);
		}

		public IEnumerable<TModel> Get<TModel>(string navigationPropertyPath)
		{
			var rawList = _context.Set<TDb>()
				.Include(navigationPropertyPath);
			return _mapper.Map<IEnumerable<TModel>>(rawList);
		}

		public async Task<TDb> GetAsync<Tid>(Tid id)
		{
			return await _context.FindAsync<TDb>(id);
		}

		public async Task<TModel> GetAsync<Tid, TModel>(Tid id)
		{
			var raw = await _context.FindAsync<TDb>(id);
			return _mapper.Map<TModel>(raw);
		}

		public async Task AddAsync(TDb entry)
		{
			await _context.AddAsync(entry);
			await _context.SaveChangesAsync();
		}

		public async Task AddAsync<TModel>(TModel model)
		{
			var raw = _mapper.Map<TDb>(model);
			await _context.AddAsync(raw);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateAsync(TDb entry)
		{
			_context.Update(entry);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateAsync<TModel>(TModel model)
		{
			var raw = _mapper.Map<TDb>(model);
			_context.Update(raw);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync<T>(T id)
		{
			var entry = await _context.FindAsync<TDb>(id);
			if (entry == null)
				return;

			_context.Remove(entry);
			await _context.SaveChangesAsync();
		}
	}
}
