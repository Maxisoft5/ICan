using ICan.Common.Domain;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Data.Repositories
{
	public class AssemblyRepository : BaseRepository, IAssemblyRepository
	{
		public AssemblyRepository(ApplicationDbContext context) : base(context)
		{
		}

		public async Task Add(OptAssembly raw)
		{
			await _context.AddAsync(raw);
			await _context.SaveChangesAsync();
		}

		public async Task<OptAssembly> GetAssembly(long assemblyId)
		{
			return await _context.OptAssembly.Where(x => x.AssemblyId == assemblyId)
				.Include(x => x.AssemblySemiproducts)
					.ThenInclude(x => x.PrintOrderSemiproduct.SemiProduct)
					.ThenInclude(x => x.SemiproductType)
				.Include(x => x.AssemblySemiproducts)
					.ThenInclude(x => x.PrintOrderSemiproduct.PrintOrder)
				.Include(x => x.AssemblySemiproducts)
					.ThenInclude(x => x.NotchOrder)
				.Include(x => x.Product)
					.ThenInclude(prod => prod.Country)
				.FirstOrDefaultAsync();
		}
	}
}
