using ICan.Common.Domain;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Data.Repositories
{
	public class SemiproductRepository : BaseRepository, ISemiproductRepository
	{
		public SemiproductRepository(ApplicationDbContext context) : base(context)
		{
		}

		public IQueryable<OptSemiproductType> GetSemiproductTypes()
		{
			var list = _context.OptSemiproductType;
			return list;
		}	
		
		public IQueryable<OptSemiproduct> GetSemiproductsWithTypes()
		{
			var list = _context.OptSemiproduct
				.Include(semiproduct=> semiproduct.SemiproductType);
			return list;
		}

		public IQueryable<OptSemiproduct> Get()
		{
			var list = _context.OptSemiproduct
				.Include(prod => prod.SemiproductPapers)
					.ThenInclude(semiproductPaper => semiproductPaper.Paper)
				.Include(prod => prod.Format)
				.Include(prod => prod.Product)
					.ThenInclude(prod => prod.ProductSeries)
				.Include(prod => prod.Product)
					.ThenInclude(prod => prod.Country)
				.Include(prod => prod.SemiproductType)
				.Include(prod => prod.BlockType)

				.OrderBy(prod => prod.SemiproductTypeId)
				.ThenBy(prod => prod.Product.ProductSeries.Order)
				.ThenBy(prod => prod.Product.DisplayOrder);
			return list;
		}

		public async Task<OptSemiproduct> GetAsync(int id)
		{
			return await _context.OptSemiproduct
				.Include(semiproduct => semiproduct.SemiproductPapers)
					.ThenInclude(semiproductPaper => semiproductPaper.Paper)
				.Include(semiproduct => semiproduct.Product.Country)
				.Include(semiproduct => semiproduct.Product.ProductSeries)
				.Include(semiproduct => semiproduct.Format)
				.Include(semiproduct => semiproduct.SemiproductType)
				.Include(semiproduct => semiproduct.RelatedProducts)
					.ThenInclude(prod => prod.Product)
					.ThenInclude(prod => prod.Country)
				.FirstOrDefaultAsync(semiproduct => semiproduct.SemiproductId == id);
		}

		public IQueryable<OptSemiproductPaper> GetSemiproductPapers()
		{
			return _context.OptSemiproductPaper;
		}

		public IQueryable<OptFormat> GetFormat()
		{
			return _context.OptFormat;
		}

		public IQueryable<OptPaper> GetPaper()
		{
			return _context.OptPaper;
		}

		public IQueryable<OptSemiproduct> FindDuplicate(int semiproductId, int semiproductTypeId, int productId, string name = "")
		{
			return _context.OptSemiproduct.Where(x => x.SemiproductId != semiproductId
										  && x.SemiproductTypeId == semiproductTypeId
										  && x.ProductId == productId
										  && (semiproductTypeId == (int)SemiProductType.Box ? x.Name.Equals(name) : true));
		}

		public async Task Add(OptSemiproduct raw)
		{
			await _context.AddAsync(raw);
			await _context.SaveChangesAsync();
		}
	}
}
