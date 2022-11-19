using ICan.Business.Managers;
using ICan.Common.Domain;
using ICan.Test.Helpers;
using ICan.Test.ParseReports;
using System.Threading.Tasks;
using Xunit;

namespace ICan.Test.ProductSeries
{
	[Collection("Context collection")]
	public class ProductSeriesTest
	{
		private HelperBase _helper;
		private CommonManager<OptProductseries> _productSeriesManager;

		public ProductSeriesTest(DbContextFixture contextFixture)
		{
			_helper = new HelperBase(contextFixture.Context);
			_productSeriesManager = _helper.GetCommonManager<OptProductseries>();
		}

		[Fact]
		public async Task CreateGet_ProductSeries_Test()
		{
			var tryGetProductSeries = await CreateAndGetProductSeries(1);
			Assert.NotNull(tryGetProductSeries);			
		}

		[Fact]
		public async Task CreateUpdate_ProductSeries_Test()
		{
			var tryGetProductSeries = await CreateAndGetProductSeries(2);
			var updatedName = "ProductSeriesUpdated";
			tryGetProductSeries.Name = updatedName;
			await _productSeriesManager.UpdateAsync(tryGetProductSeries);
			tryGetProductSeries = await _productSeriesManager.GetAsync(2);
			Assert.Equal(updatedName, tryGetProductSeries.Name);
		}

		[Fact]
		public async Task CreateDelete_ProductSeries_Test()
		{
			var tryGetProductSeries = await CreateAndGetProductSeries(3);
			await _productSeriesManager.DeleteAsync(3);
			tryGetProductSeries = await _productSeriesManager.GetAsync(3);
			Assert.Null(tryGetProductSeries);
		}

		private async Task<OptProductseries> CreateAndGetProductSeries(int id)
		{
			var productSeries = new OptProductseries
			{
				ProductSeriesId = id
			};
			await _productSeriesManager.AddAsync(productSeries);
			return await _productSeriesManager.GetAsync(id);
		}
	}
}
