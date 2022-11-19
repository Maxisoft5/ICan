using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using ICan.Common.Domain;

namespace ICan.Controllers
{
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class CountRedirectController : BaseController
	{
		private readonly CommonManager<OptUtmStatistics> _utmManager;
		private readonly string _goalWbPage;
		private readonly string _goalOzonPage;
		private const string _wb = "wb";

		public CountRedirectController(
			UserManager<ApplicationUser> userManager,
			CommonManager<OptUtmStatistics> utmManager,
			ILogger<BaseController> logger,
			IConfiguration configuration) : base(userManager, logger, configuration)
		{
			_utmManager = utmManager;
			_goalWbPage = _configuration["Settings:Campaign:WbUrl"];
			_goalOzonPage = _configuration["Settings:Campaign:OzonUrl"];
		}

		[Authorize(Roles = "Admin,Operator")]

		public async Task<IActionResult> Results()
		{
			var list = await _utmManager.GetAsync();
			return View(list);
		}

		[AllowAnonymous]
		public async Task<IActionResult> Index(string utm_source, string utm_medium, string utm_campaign, string utm_content = null, string utm_term = null)
		{
			await _utmManager.AddAsync(new OptUtmStatistics
			{
				Date = DateTime.UtcNow,
				UtmSource = utm_source,
				UtmMedium = utm_medium,
				UtmCampaign = utm_campaign,
				UtmContent = utm_content,
				UtmTerm = utm_term
			});

			if (utm_campaign.Equals(_wb, StringComparison.InvariantCultureIgnoreCase))
				return Redirect(_goalWbPage);

			return Redirect(_goalOzonPage);
		}

		[Authorize(Roles = "Admin,Operator")]
		[HttpPost]
		public async Task<IActionResult> Download()
		{
			var list = await _utmManager.GetAsync();
			var bytes = GetReport(list);
			return File(bytes, MediaTypeNames.Application.Octet, $"Статистика переходов по состоянию на {DateTime.Now.ToShortDateString()}.xlsx");
		}

		private byte[] GetReport(IEnumerable<OptUtmStatistics> list)
		{
			byte[] bytes = new byte[0];
			using (ExcelPackage objExcelPackage = new ExcelPackage())
			{
				ExcelWorksheet objWorksheet = objExcelPackage.Workbook.Worksheets.Add("Заказ");

				var current = 1;

				objWorksheet.Cells[current, 1].Value = "Дата";

				objWorksheet.Cells[current, 2].Value = "Utm source";
				objWorksheet.Cells[current, 3].Value = "Utm medium";
				objWorksheet.Cells[current, 4].Value = "Utm campaigh";
				objWorksheet.Cells[current, 5].Value = "Utm content";
				objWorksheet.Cells[current, 6].Value = "Utm term";

				foreach (var entry in list)
				{
					current++;
					objWorksheet.Cells[current, 1].Style.Numberformat.Format
						   = "dd.mm.yyyy";
					objWorksheet.Cells[current, 1].Value = entry.Date.ToShortDateString();
					objWorksheet.Cells[current, 2].Value = entry.UtmSource;
					objWorksheet.Cells[current, 3].Value = entry.UtmMedium;
					objWorksheet.Cells[current, 4].Value = entry.UtmCampaign;
					objWorksheet.Cells[current, 5].Value = entry.UtmContent;
					objWorksheet.Cells[current, 6].Value = entry.UtmTerm;


				}
				objWorksheet.Column(1).Width = 15;
				objWorksheet.Column(2).Width = 15;
				objWorksheet.Column(3).Width = 15;
				objWorksheet.Column(4).Width = 15;
				objWorksheet.Column(5).Width = 15;
				objWorksheet.Column(6).Width = 15;

				bytes = objExcelPackage.GetAsByteArray();
			}
			return bytes;
		}
	}
}
