using ICan.Business.Managers;
using ICan.Common.Models.Opt.Report;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ICan.Test.ParseReports
{
	[Collection("Context collection")]
	public class ParseOzonReportTest
	{
		private DbContextFixture contextFixture;
		private ReportManager _reportManager;
		private ReportHelper helper;
		private string ozonReportPath = "./Ethalon/OZON.xlsx";

		public ParseOzonReportTest(DbContextFixture contextFixture)
		{
			this.contextFixture = contextFixture;
			helper = new ReportHelper(contextFixture.Context);
		}

		[Fact]
		public async Task ParseOzonReport_GivingEthalonReport_IsEqualResult()
		{
			_reportManager = helper.GetReportManager();
			var file = new FileInfo(ozonReportPath);
			var iFormFile = new FormFile(file.OpenRead(), 0, file.OpenRead().Length, file.Name, file.Name);
			var reportResult = await _reportManager.ParseFileAsync(iFormFile);
			Assert.True(reportResult.ReportItems.Count == 26);
			Assert.True(reportResult.ReportNum == "214651");
			Assert.True(reportResult.ReportDate.Value.Date == new DateTime(2020, 02, 29));
			Assert.True(reportResult.TotalSum.Equals(191910.10));
		}

		[Fact]
		public async Task  ParseOzonReport_GivingEthalonReport_IsOzonReport()
		{
			var report = await helper.GetReport(ozonReportPath);
			var result = report as OzonReport;
			Assert.NotNull(result);
		}	
	}
}
