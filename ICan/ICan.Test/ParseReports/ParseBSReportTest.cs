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
	public class ParseBSReportTest
	{
		private ReportManager _reportManager;
		private ReportHelper _helper;
		private const string _bsReportPath = "./Ethalon/BS.xlsx";

		public ParseBSReportTest(DbContextFixture contextFixture)
		{
			_helper = new ReportHelper(contextFixture.Context);
		}

		[Fact]
		public async Task ParseMyShopReport_GivingEthalonReport_IsEqualResult()
		{
			_reportManager = _helper.GetReportManager();
			var file = new FileInfo(_bsReportPath);
			var iFormFile = new FormFile(file.OpenRead(), 0, file.OpenRead().Length, file.Name, file.Name);
			var reportResult = await _reportManager.ParseFileAsync(iFormFile);
			Assert.True(reportResult.ReportItems.Count == 24);
			Assert.True(reportResult.ReportDate.Value.Date == new DateTime(2020, 03, 31));
			Assert.True(reportResult.TotalSum.Equals(125768.42));
		}	

		[Fact]
		public async Task ParseMyShopReport_GivingEthalonReport_IsMyShopReport()
		{
			var report = await _helper.GetReport(_bsReportPath);
			var result = report as MyShopReport;
			Assert.NotNull(result);
		}
	}
}
