using ICan.Business.Managers;
using ICan.Common.Domain;
using ICan.Test.Fakes;
using Moq;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ICan.Test.Semiproduct
{
	[Collection("Context collection")]
	public class SemiproductTest
	{
		private SemiproductManager _semiproductManager;

		[Fact]
		public async Task CreateSemiproduct_GiveNonUniqueSemiproduct_GetError()
		{
			InitManagersAndRepositories();
			//arrange 
			var semiproduct =
				new Common.Models.Opt.SemiproductModel { ProductId = 1, SemiproductId = 1, SemiproductPapers = Enumerable.Empty<OptSemiproductPaper>() };

			await _semiproductManager.Create(semiproduct);
			//act
			var ok = _semiproductManager.CheckModel(semiproduct, out var error);

			//assert
			Assert.False(ok);
		}

		private void InitManagersAndRepositories()
		{
			var productManager = new Mock<ProductManager>();
			_semiproductManager = new SemiproductManager(null, null, new FakeSemiproductRepository(), null, null);
		}
	}
}
