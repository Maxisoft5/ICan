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
	public class ParseWBReportTest
	{
		private DbContextFixture contextFixture;
		private ReportManager _reportManager;
		private ReportHelper helper;
		private string wbReportPath = "./Ethalon/WB.xlsx";
		public ParseWBReportTest(DbContextFixture contextFixture)
		{
			this.contextFixture = contextFixture;
			helper = new ReportHelper(contextFixture.Context);
		}

		[Fact]
		public async Task ParseWbReport_GivingEthalonReport_IsEqualResult()
		{
			_reportManager = helper.GetReportManager();
			var file = new FileInfo(wbReportPath);
			var iFormFile = new FormFile(file.OpenRead(), 0, file.OpenRead().Length, file.Name, file.Name);
			var reportResult = await _reportManager.ParseFileAsync(iFormFile);
			Assert.True(reportResult.ReportItems.Count == 26);
			Assert.True(reportResult.ReportNum.Equals("1137702"));
			Assert.Equal(new DateTime(2020, 01, 31), reportResult.ReportDate.Value.Date);
			Assert.Equal(2131828.49, reportResult.TotalSum);
		}

		[Fact]
		public async Task ParseOzonReport_GivingEthalonReport_IsWbReport()
		{
			var report = await helper.GetReport(wbReportPath);
			var result = report as WbReport;
			Assert.NotNull(result);
		}

	}
}
