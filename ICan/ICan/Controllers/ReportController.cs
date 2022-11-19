using AutoMapper;
using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Models.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mime;
using System.Threading.Tasks;
using ICan.Common;
using ICan.Common.Models.Opt;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,Operator,StoreKeeper")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class ReportController : BaseController
	{
		private readonly ProductManager _productManager;

		private readonly ReportManager _reportManager;

		public ReportController(IMapper mapper,
			UserManager<ApplicationUser> userManager,
			ILogger<BaseController> logger,
			ProductManager productManager,
			ReportManager reportManager,
			IConfiguration configuration)
            : base(mapper, userManager, logger, configuration)
		{
			_productManager = productManager;
			_reportManager = reportManager;
		}

		public async Task<IActionResult> ReportsPartial()
		{
			var list = await _reportManager.GetReportsPartial();

			SetMonthsYears();
			return PartialView("_ReportList", list);
		}

		public IActionResult DownloadReportPartial()
		{
			SetMonthsYears();
			return PartialView("_DownloadReport", new DownloadReportModel());
		}

		[HttpPost]
		public async Task<ActionResult> DownloadReport(int fromMonth, int fromYear, int toMonth, int toYear)
		{
			var result = await _reportManager.DownloadReport(fromMonth, fromYear, toMonth, toYear);
			return File(result, MediaTypeNames.Application.Octet,
				$"Сводный отчет c {fromMonth:00}.{fromYear} по {toMonth:00}.{toYear}.xlsx");
		}

		public async Task<IActionResult> Index()
		{
			var reports = await _reportManager.GetReports();

			SetMonthsYears();

			return View(reports);
		}

		[Authorize(Roles = "Admin,Operator")]
		public IActionResult UploadedUpds() => View();
		 

		public IActionResult GetUPDReports(TableOptions options)
		{
			
			var tableResult = _reportManager.GetUpdReports(options);

			return Ok(new { total = tableResult.Total, rows = tableResult.Rows});
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UploadReport(IFormCollection collection)
		{
			if (collection.Files == null || collection.Files.Count == 0)
			{
				_logger.LogError(Const.ErrorMessages.NoFile);
				TempData["ErrorMessage"] = Const.ErrorMessages.NoFile;
			}
			else
			{
				try
				{
					await _reportManager.UploadReport(collection);
					TempData["StatusMessage"] = "Отчёт успешно загружен";
				}
				catch (UserException ex)
				{
					_logger.LogError(ex, "ой");
					TempData["ErrorMessage"] = ex.Message;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "ой");
					TempData["ErrorMessage"] = "При загрузке отчёта возникли проблемы";
				}
			}
			return RedirectToAction("Index");
		}


		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(string id)
		{
			try
			{
				await _reportManager.RemoveReportById(id);

				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.GetType().ToString());
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteForUser;
			}

			return RedirectToAction(nameof(Index));
		}

		public async Task<ActionResult> Details(string id)
		{
			var details = await _reportManager.GetReportDetails(id);

			return View(details);
		}

		 

		public IActionResult LeonardoIndex()
		{
			SetLeonardoViewData();
			ViewData["OnlyXlsx"] = true;
			return View("FileTransform");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> LeonardoIndex(IFormCollection collection)
		{
			if (collection.Files == null || collection.Files.Count == 0)
			{
				_logger.LogError(Const.ErrorMessages.NoFile);
				TempData["ErrorMessage"] = Const.ErrorMessages.NoFile;
			}
			else
			{
				try
				{
					var file = collection.Files[0];
					var uploadedData = await _reportManager.ParseLeonardoFileAsync(file);

					if (uploadedData == null)
						return RedirectToAction("Error");

					var bytes = _reportManager.GetLeonardoTotalReport(uploadedData);
					return File(bytes, MediaTypeNames.Application.Octet,
					 $"Сводный заказ по Леонардо {DateTime.Now.ToShortDateString()}.xlsx");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "ой");
					TempData["ErrorMessage"] = ex.ToString();
				}
			}
			SetLeonardoViewData();
			return View("FileTransform");
		}

		private void SetLeonardoViewData()
		{
			ViewData["Title"] = "Заказ от Леонардо";
			ViewData["Description"] = "Загрузить  и преобразовать заказ от Леонародо";
			ViewData["PostAction"] = nameof(LeonardoIndex);
		}

		//private void SetWBViewData()
		//{
		//	ViewData["Title"] = "Поставка в ВБ";
		//	ViewData["Description"] = "Загрузить и преобразовать файл для ВБ";
		//	ViewData["PostAction"] = nameof(WbShipment);
		//}
	}
}
