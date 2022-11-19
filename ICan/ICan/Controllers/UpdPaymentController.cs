using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using ICan.Common;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,Operator")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class UpdPaymentController : BaseController
	{
		private readonly UpdPaymentManager _updPaymentManager;

		public UpdPaymentController(UpdPaymentManager updPaymentManager,
			ILogger<BaseController> logger
			) : base(logger)
		{
			_updPaymentManager = updPaymentManager;
		}

		public async Task<IActionResult> Index()
		{
			var data = await _updPaymentManager.GetAsync();
			return View(data);
		}

		public async Task<IActionResult> CarriedIndex()
		{
			var data = await _updPaymentManager.GetCarriedAsync();
			return View(data);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult DeleteCarriedPayment(int id)
		{
			try
			{
				_updPaymentManager.DeleteCarriedPayment(id);
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			}
			catch (Exception ex) {
				_logger.LogError(ex, Const.ErrorMessages.CantDelete);
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteForUser;
			}
			return RedirectToAction(nameof(CarriedIndex));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult CarryPayment(string reportNums, string reportYear)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(reportNums) || string.IsNullOrWhiteSpace(reportYear)
					|| !int.TryParse(reportYear, out var parsedYear))
				{
					throw new UserException("Номер упд или год упд не определён");
				}
				var reportIds = reportNums.Split(",");
				_updPaymentManager.CarryPayment(reportIds, parsedYear);

				TempData["StatusMessage"] = "Оплата упд с " +
					(reportIds.Count() > 1 ? "номерами" : "номером") +
					 $" {reportNums} за {reportYear} проведена";
			}
			catch (UserException ex)
			{
				_logger.LogError(ex, "ой");
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "ой");
				TempData["ErrorMessage"] = "При загрузке файлов возникли проблемы";
			}
			return RedirectToAction(nameof(Index));
		}
	}
}