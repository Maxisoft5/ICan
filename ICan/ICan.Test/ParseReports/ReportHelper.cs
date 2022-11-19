using AutoFixture;
using ICan.Business.Managers;
using ICan.Business.Services;
using ICan.Common.Models.Opt.Report;
using ICan.Data.Context;
using ICan.Test.Helpers;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System.IO;
using System.Threading.Tasks;

namespace ICan.Test.ParseReports
{
	public class ReportHelper: HelperBase
	{
		public ReportHelper(ApplicationDbContext context) : base(context) { }

		public async Task<Report> GetReport(string path)
		{
			var fixture = GetFixture();
			var fileInfo = new FileInfo(path);
			var _parseService = new ReportParseService(
						 fixture.Create<ApplicationDbContext>(),
						 fixture.Create<ReportCriteriaService>(),
						 fixture.Create<ProductManager>(),
						 fixture.Create<ILogger<ReportParseService>>());

			using (FileStream ms = fileInfo.OpenRead())
			using (var packege = new ExcelPackage(ms))
			using (var worksheet = packege.Workbook.Worksheets[0])
			{
				var report = await _parseService.GetShopReport(worksheet);
				return report;
			}
		}

		public ReportManager GetReportManager()
		{
			var fixture = GetFixture();

			return new ReportManager(null,
				fixture.Create<ApplicationDbContext>(),
				fixture.Create<ILogger<BaseManager>>(),
				fixture.Create<ProductManager>(),
				fixture.Create<WarehouseJournalManager>(),
				null,
				null, null, fixture.Create<ReportParseService>()); ;
		}
	}
}
