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
	public class ParseWBPeriodReport
	{
		private DbContextFixture contextFixture;
		private ReportManager _reportManager;
		private ReportHelper helper;
		private string wbReportPath = "./Ethalon/WB-period.xlsx";
		public ParseWBPeriodReport(DbContextFixture contextFixture)
		{
			this.contextFixture = contextFixture;
			helper = new ReportHelper(contextFixture.Context);
		}

		[Fact]
		public async Task ParseWbPeriodReport_GivingEthalonReport_IsEqualResult()
		{
			_reportManager = helper.GetReportManager();
			var file = new FileInfo(wbReportPath);
			var iFormFile = new FormFile(file.OpenRead(), 0, file.OpenRead().Length, file.Name, file.Name);
			var reportResult = await _reportManager.ParseFileAsync(iFormFile);
			Assert.Equal(25, reportResult.ReportItems.Count);
			Assert.True(reportResult.ReportNum.Equals("1496132"));
			Assert.Equal(new DateTime(2020, 06, 07), reportResult.ReportDate.Value.Date);
			Assert.Equal(711169.29, reportResult.TotalSum);
			Assert.Equal(new DateTime(2020, 06, 01), reportResult.ReportPeriodFrom);
			Assert.Equal(new DateTime(2020, 06, 07), reportResult.ReportPeriodTo);
		}

		[Fact]
		public async Task ParseWBPeriodReport_GivingEthalonReport_IsWbPeriodReport()
		{
			var report = await helper.GetReport(wbReportPath);
			var result = report as WBReportByPeriod;
			Assert.NotNull(result);
		}

	}
}
