using ICan.Common.Domain;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using ICan.Common.Utils;
using System.Threading.Tasks;

namespace ICan.Test.Fakes
{
	public class FakeSemiproductRepository : ISemiproductRepository
	{
		private List<OptSemiproduct> Entries = new List<OptSemiproduct>();
		private List<OptPaper> Papers = new List<OptPaper>() { };
		private List<OptFormat> Formats = new List<OptFormat>();

		public IQueryable<OptSemiproductType> GetSemiproductTypes()
		{
			var types = Enum.GetValues(typeof(SemiProductType)).Cast<SemiProductType>()
				.Select(sType =>
					{
						var optSType = new OptSemiproductType
						{
							SemiproductTypeId = (int)sType,
							Name = sType.GetDisplayName()
						}; return optSType;
					});
			return types.AsQueryable();
		}

		public IQueryable<OptSemiproduct> Get()
		{
			return Entries.AsQueryable();
		}

		public async Task<OptSemiproduct> GetAsync(int id)
		{
			return Entries.FirstOrDefault(semiproduct => semiproduct.SemiproductId == id);
		}

		public IQueryable<OptSemiproductPaper> GetSemiproductPapers()
		{
			return Entries?.SelectMany(smPaper => smPaper.SemiproductPapers).AsQueryable();
		}

		public IQueryable<OptFormat> GetFormat()
		{
			return Formats.AsQueryable();
		}

		public IQueryable<OptPaper> GetPaper()
		{
			return Papers.AsQueryable();
		}

		public IQueryable<OptSemiproduct> FindDuplicate(int semiproductId, int semiproductTypeId, int productId, string name = "")
		{
			return Entries.Where(x => x.SemiproductId != semiproductId
											 && x.SemiproductTypeId == semiproductTypeId
											 && x.ProductId == productId
											 && (semiproductTypeId == (int)SemiProductType.Box ? x.Name.Equals(name) : true)).AsQueryable();
		}

		public async Task Add(OptSemiproduct raw)
		{
			Entries.Add(raw);
		}

		public IQueryable<OptSemiproduct> GetSemiproductsWithTypes()
		{
			throw new NotImplementedException();
		}
	}
}
