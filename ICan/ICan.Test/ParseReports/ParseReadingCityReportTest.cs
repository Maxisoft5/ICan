using ICan.Business.Managers;
using ICan.Common.Models.Opt.Report;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.IO;
using System.Threading.Tasks;
using Xunit;


namespace ICan.Test.ParseReports
{
	[Collection("Context collection")]
	public class ParseReadingCityReportTest
	{
		private DbContextFixture contextFixture;
		private ReportManager _reportManager;
		private ReportHelper helper;
		private string readingCityReportPath = "./Ethalon/ReadingCity.xlsx";
		public ParseReadingCityReportTest(DbContextFixture contextFixture)
		{
			this.contextFixture = contextFixture;
			helper = new ReportHelper(contextFixture.Context);
		}

		[Fact]
		public async Task ParseReadingCityReport_GivingEthalonReport_IsEqualResult()
		{
			_reportManager = helper.GetReportManager();
			var file = new FileInfo(readingCityReportPath);
			var iFormFile = new FormFile(file.OpenRead(), 0, file.OpenRead().Length, file.Name, file.Name);
			var reportResult = await _reportManager.ParseFileAsync(iFormFile);
			Assert.True(reportResult.ReportItems.Count == 25);
			Assert.Equal(70226.92, reportResult.TotalSum);
		}

		[Fact]
		public async Task ParseReadingCityReport_GivingEthalonReport_IsReadingCityReport()
		{
			var report = await helper.GetReport(readingCityReportPath);
			var result = report as ReadingCityReport;
			Assert.NotNull(result);
		}
	}
}
