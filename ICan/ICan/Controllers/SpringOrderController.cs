using ICan.Business.Managers;
using ICan.Common;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Controllers
{
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class SpringOrderController : BaseController
	{
		private readonly SpringManager _springManager;
		private readonly SpringOrderManager _springOrderManager;
		public SpringOrderController(ILogger<BaseController> logger, SpringManager springManager,
			SpringOrderManager springOrderManager) : base(logger)
		{
			_springManager = springManager;
			_springOrderManager = springOrderManager;
		}

		public async Task<IActionResult> Index()
		{
			var springOrders = _springOrderManager.GetSpringOrders();
			return View(springOrders);
		}

		public async Task<IActionResult> Create()
		{
			await SetSpringsList();
			var model = new SpringOrderModel
			{
				OrderDate = DateTime.Now
			};
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Create(SpringOrderModel springOrderModel)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await _springOrderManager.Create(springOrderModel);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index));
				}
				catch (Exception ex)
				{
					_logger.LogError(ex.Message);
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSave;
				}
			}
			await SetSpringsList();
			return View();
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var model = await _springOrderManager.GetById(id);
			ViewData["SpringOrderId"] = id;
			await SetSpringsList();
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(SpringOrderModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await _springOrderManager.Edit(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index));
				}
				catch (Exception ex)
				{
					_logger.LogError(ex.Message);
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSave;
				}
			}
			ViewData["SpringOrderId"] = model.SpringOrderId;
			await SetSpringsList();
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _springOrderManager.Delete(id);
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			}
			catch (UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteForUser;
			}
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> AddIncoming(int id)
		{
			var order = await _springOrderManager.GetById(id);

			var model = new SpringOrderIncomingModel
			{
				SpringOrderId = id,
				IncomingDate = DateTime.Now,
				SpringNumerOfTurns = order.Spring.NumberOfTurns.NumberOfTurns
			};
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> AddIncoming(SpringOrderIncomingModel springOrderIncoming)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await _springOrderManager.AddIncoming(springOrderIncoming);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				}
				catch
				{
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				}
			}
			return RedirectToAction(nameof(Edit), new { id = springOrderIncoming.SpringOrderId });
		}

		[HttpPost]
		public async Task<IActionResult> DeleteIncoming(int incomingId)
		{
			try
			{
				await _springOrderManager.DeleteIncoming(incomingId);
				return Ok();
			}
			catch (UserException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "При удалении прихода возникла ошибка");
				return BadRequest();
			}
		}

		[HttpPost]
		public async Task<IActionResult> AddPayment(decimal amount, string date, int springOrderId)
		{
			if (amount > 0 && DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None,
				out var paymentDate))
			{
				var model = new PaymentModel { Amount = amount, PaymentDate = paymentDate, OrderId = springOrderId };
				model.PaymentId = await _springOrderManager.AddPayment(model);
				return Ok(model);
			}

			return BadRequest();
		}

		[HttpDelete]
		[Authorize(Roles = "Admin,Assembler")]
		public async Task<IActionResult> DeletePayment(int paymentId)
		{
			await _springOrderManager.DeletePayment(paymentId);
			return Ok();
		}

		private async Task SetSpringsList()
		{
			ViewData["SpringList"] = (await _springManager.GetSprings()).Select(x => new SelectListItem { Text = x.SpringName, Value = x.SpringId.ToString() });
		}
	}
}
