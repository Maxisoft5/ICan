using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using ICan.Common.Models.Opt;
using ICan.Common;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,Assembler")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class NotchOrderController : BaseController
	{
		private readonly NotchOrderManager _notchOrderManager;

		public NotchOrderController(ILogger<NotchOrderController> logger, NotchOrderManager notchOrderManager) : base(logger)
		{
			_notchOrderManager = notchOrderManager;
		}

		public async Task<IActionResult> Index()
		{
			var orders = await _notchOrderManager.GetOrders();
			return View(orders);
		}

		public IActionResult Create()
		{
			var model = new NotchOrderModel { OrderDate = DateTime.Now };
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Create(NotchOrderModel notchOrder)
		{
			try
			{
				await _notchOrderManager.Create(notchOrder);
				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
			}
			catch (UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
				return View(notchOrder);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
			}
			return RedirectToAction("Index");
		}

		public async Task<IActionResult> Edit(int id)
		{
			var order = await _notchOrderManager.GetById(id);
			return View(order);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(NotchOrderModel notchOrder)
		{
			try
			{
				await _notchOrderManager.Update(notchOrder);
				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
			}
			catch (UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
				return View(notchOrder);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
			}
			return RedirectToAction("Index");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _notchOrderManager.Delete(id);
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteForUser;
			}
			return RedirectToAction("Index");
		}

		public async Task<IActionResult> GetPrintOrders(string existingIds, int? currentNotchOrderId = null)
		{
			var existingList = existingIds?.Split(",").Select(item => int.Parse(item)) ?? Enumerable.Empty<int>();
			var printOrders = await _notchOrderManager.GetPrintOrdersForNotch(currentNotchOrderId);
			var result = printOrders
							.Where(printOrder => !existingList.Contains(printOrder.PrintOrderId))
							.Select(printOrder => new
							{
								Name = printOrder.DisplayName,
								Id = printOrder.PrintOrderId
							});
			return Ok(result);
		}

		public async Task<IActionResult> AddIncoming(int id)
		{
			var model = new NotchOrderIncomingModel
			{
				IncomingDate = DateTime.Now,
				NotchOrderId = id,
				IncomingItems = await _notchOrderManager.GetNotchOrderIncomingsByNotchOrderId(id)
			};
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> AddIncoming(NotchOrderIncomingModel incoming)
		{
			await _notchOrderManager.AddIncoming(incoming);
			TempData["StatusMessage"] = Const.SuccessMessages.Saved;
			return RedirectToAction("Edit", new { id = incoming.NotchOrderId });
		}

		public async Task<IActionResult> DeleteIncoming(int id)
		{
			await _notchOrderManager.DeleteIncoming(id);
			return Ok();
		}
	}
}
