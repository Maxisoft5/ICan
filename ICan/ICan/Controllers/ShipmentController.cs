using AutoMapper;
using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,Operator")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class ShipmentController : BaseController
	{
		private readonly ProductManager _productManager;

		private readonly ReportManager _reportManager;

		public ShipmentController(IMapper mapper, 
			UserManager<ApplicationUser> userManager,
			ILogger<BaseController> logger,
			ProductManager productManager,
			 ReportManager reportManager
			) : base(mapper, userManager, logger)
		{
			_productManager = productManager;
			_reportManager = reportManager;
		}

		public ActionResult Index()
		{
			SetMonthsYears(true);
			return View("Index");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<FileResult> Index(IFormCollection collection)
		{
			try
			{
				var products = await _productManager.GetAsync(false, dontShowDisabled: false);
				int.TryParse(collection["year"], out var year);

				var helper = new ReportHelper
				{
					FromYear = year,
					ToYear = year,
					Products = products
				};

				var bytes = await _reportManager.GetShipmentReport(helper);

				return File(bytes, MediaTypeNames.Application.Octet,
					$"Отчёт по отгрузкам за {year}.xlsx");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, null);
				throw;
			}
		}

		public async Task<FileResult> GetReportForPrimer(int year)
		{
			var bytes = await _reportManager.GetPrimerReport(year, BobReportTypeEnum.QuantityAndSum);
			return File(bytes, MediaTypeNames.Application.Octet,
				$"Отчёт по отгрузкам букваря за {year}.xlsx");
		}

		public async Task<FileResult> GetReportForPrimerQty(int year)
		{
			var bytes = await _reportManager.GetPrimerReport(year, BobReportTypeEnum.Quantity);
			return File(bytes, MediaTypeNames.Application.Octet,
				$"Отчёт по отгрузкам букваря за {year}(кол-во).xlsx");
		}
	}
}