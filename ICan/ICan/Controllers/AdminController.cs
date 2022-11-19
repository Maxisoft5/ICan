using ICan.Jobs.OneC;
using ICan.Jobs.WB;
using ICan.Business.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Net.Mime;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using HtmlAgilityPack;
using System.Threading;
using System.IO;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,Operator")]
	public class AdminController : BaseController
	{
		private readonly OneCImportJob _oneCJob;
		private readonly WbOrdersImportJob _wbOrdersJob;
		private readonly WbSalesImportJob _wbSalesJob;
		private readonly AdminManager _adminManager;
		private readonly ReportManager _reportManager;
		private static readonly HttpClient _client = new HttpClient();

		public AdminController(
			ILogger<BaseController> logger,
			AdminManager adminManager,
			ReportManager reportManager,
			OneCImportJob oneCJob,
			WbOrdersImportJob wbOrdersJob,
			WbSalesImportJob wbSalesJob,
			IConfiguration configuration
			)
			: base(null, logger, configuration)
		{
			_oneCJob = oneCJob;
			_wbOrdersJob = wbOrdersJob;
			_wbSalesJob = wbSalesJob;
			_adminManager = adminManager;
			_reportManager = reportManager;
		}

		public IActionResult Index() => View();

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult RunJobNow()
		{
			_oneCJob.Import();
			return Ok("import is done");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> OzonExperiment()
		{
			var html = @"https://www.ozon.ru/publisher/ya-mogu-70816609/";

			var sleep = int.Parse(_configuration["Settings:Experiment"]);
			var times = int.Parse(_configuration["Settings:ExperimentTimes"]);
			byte[] bytes;
			for (var i = 0; i < times; i++)
			{
				Thread.Sleep(sleep);
				try
				{
					using (var file = await _client.GetStreamAsync(html).ConfigureAwait(false))
					using (var memoryStream = new MemoryStream())
					{
						await file.CopyToAsync(memoryStream);
						bytes = memoryStream.ToArray();
					}
					string result = System.Text.Encoding.UTF8.GetString(bytes);
					var response = await _client.GetAsync(html);
					//response.EnsureSuccessStatusCode();
					var body = await response.Content.ReadAsStringAsync();
					var htmlDoc = new HtmlDocument();
					htmlDoc.LoadHtml(body);

					var node = htmlDoc.DocumentNode.SelectNodes(@"//button//div[contains(string(), ""В корзину"")]");
					_logger.LogWarning($"[ozon] {i} {node?.Count}");

					//return Ok("import is done");
				}
				catch (Exception ex)
				{
					_logger.LogWarning($"[ozon] {i} {ex.ToString()}");
					_logger.LogError(ex.ToString());
				}
			}
			return Ok();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult RunWbJob(string jobName, DateTime minDate)
		{
			if (string.IsNullOrWhiteSpace(jobName))
			{
				return BadRequest();
			}

			if (jobName.Equals("Заказы", StringComparison.InvariantCultureIgnoreCase))
			{
				_wbOrdersJob.ImportCustom(minDate);
			}
			else
			{
				_wbSalesJob.ImportCustom(minDate);
			}

			return Ok("import is done");
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> GetWbReport()
		{
			var startDate = DateTime.Parse(_configuration["Settings:WB:StartReportDate"]);
			var endDate = DateTime.Now.AddDays(-1);
			var result = await _reportManager.GetWbApiReportAsync(startDate, endDate);
			return File(result, MediaTypeNames.Application.Octet, $"Отчет по заказам и продажам WB.xlsx");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Migrate()
		{
			var result = _adminManager.Migrate();
			if (result)
			{
				return Ok("import is done");
			}

			return BadRequest();
		}
	}
}