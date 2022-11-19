using ICan.Common.Domain;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Data.Repositories
{
	public class DiscountRepository : BaseRepository, IDiscountRepository
	{
		public DiscountRepository(ApplicationDbContext context) : base(context)
		{
		}

		public async Task<long> Add(OptDiscount raw)
		{
			await _context.AddAsync(raw);
			await _context.SaveChangesAsync();
			return raw.DiscountId;
		}

		public async Task Archive(OptDiscount raw)
		{
			raw.IsArchived = true;
			await _context.SaveChangesAsync();
		}

		public async Task<OptDiscount> Get(long discountId)
		{
			return  await _context.OptDiscount.FirstOrDefaultAsync(m => m.DiscountId == discountId);
		}

		public IQueryable<OptDiscount> Get()
		{
			return _context.OptDiscount;
		}
	}
}
