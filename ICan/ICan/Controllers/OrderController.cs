using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Models.Exceptions;
using ICan.Common.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using ICan.Common.Models.Opt;
using ICan.Common;
using ICan.Common.Utils;

namespace ICan.Controllers
{
	[Authorize()]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class OrderController : BaseController
	{
		private readonly ProductManager _productManager;
		private readonly OrderManager _orderManager;
		private readonly OrderMailManager _mailManager;
		private readonly PriceManager _priceManager;
		private readonly EventManager _eventManager;
		private readonly ShopManager _shopManager;

		public OrderController(ProductManager productManager, EventManager eventManager,
		OrderManager orderManager, PriceManager priceManager,
			UserManager<ApplicationUser> manager,
			OrderMailManager mailManager,
			GlobalSettingManager globalSettingManager,
			ShopManager shopManager,
			ILogger<BaseController> logger) : base(manager, logger)
		{
			_productManager = productManager;
			_eventManager = eventManager;
			_priceManager = priceManager;
			_orderManager = orderManager;
			_mailManager = mailManager;
			_globalSettingManager = globalSettingManager;
			_shopManager = shopManager;
		}

		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> IndexData(TableOptions options)
		{
			var user = await _userManager.GetUserAsync(User);
			var userRoles = await _userManager.GetRolesAsync(user);
			var showAll = userRoles.Contains(Const.Roles.Admin)
						|| userRoles.Contains(Const.Roles.StoreKeeper)
						|| userRoles.Contains(Const.Roles.Operator);

			var pageSize = int.TryParse(options?.Limit, out var pageS) ? pageS : Const.PageSize;
			var offset = int.TryParse(options?.Offset, out var offS) ? offS : 0;
			var orders = _orderManager.GetOrders(User, showAll, options.Filter, pageSize, offset, out var total);
			return Ok(new { total, rows = orders });
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddPayment(OldPaymentModel model)
		{
			if (ModelState.IsValid)
			{
				await _orderManager.AddPaymentAsync(model);
			}
			else
			{
				TempData["ErrorMessage"] = GetErrors();
			}
			return RedirectToAction("Edit", new { id = model.OrderId });
		}


		[HttpPost]
		[Authorize(Roles = "Admin")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeletePayment(int id)
		{
			var orderId = await _orderManager.DeletePaymentAsync(id);
			if (orderId.HasValue) {
				return RedirectToAction("Edit", new { id = orderId.Value });
			}
			return RedirectToAction("Index");
		}

		public async Task<IActionResult> Details(string id)
		{
			if (!Guid.TryParse(id, out var guid))
			{
				return NotFound();
			}

			try
			{
				var model = await GetOrderAsync(guid);
				if (model == null)
					return NotFound();
				ViewData["OrderId"] = id;
				return View(model);
			}
			catch (UnauthorizedAccessException ex)
			{
				_logger.LogError(ex, "Предпринята попытка просмотреть чужой заказ");
				return RedirectToAction("NoRights", "Error");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "При запросе на просмотр заказа произошла ошибка");
				return RedirectToAction("Index", "Error");
			}
		}

		public async Task<IActionResult> Edit(string id)
		{
			if (!Guid.TryParse(id, out var guid))
			{
				return NotFound();
			}
			try
			{
				var model = await GetOrderAsync(guid);
				if (model == null)
					return BadRequest();

				await SetViewData(model);

				return View(model);
			}
			catch (UnauthorizedAccessException ex)
			{
				_logger.LogError(ex, "Предпринята попытка отредактировать чужой заказ");
				return RedirectToAction("NoRights", "Error");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "При запросе на редактирвание произошла ошибка");
				return RedirectToAction("Index", "Error");
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id, OrderModel orderModel)
		{
			if (id != orderModel.OrderId.ToString() || !Guid.TryParse(id, out var guid) || guid == Guid.Empty)
			{
				return NotFound();
			}

			var optOrder = await _orderManager.GetOrderAsync(guid);
			var user = await _userManager.GetUserAsync(User);
			var userRoles = await _userManager.GetRolesAsync(user);

			CheckOrderModel(orderModel, optOrder, userRoles.Contains(Const.Roles.Admin), userRoles.Contains(Const.Roles.Operator));

			if (ModelState.IsValid)
			{
				try
				{
					if (optOrder == null)
					{
						return NotFound();
					}
					var needChangeTrackNoNotify = await _orderManager.UpdateOrder(orderModel, optOrder);

					if (needChangeTrackNoNotify)
					{
						await ChangeTrackNoNotify(orderModel);
					}
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				}
				catch (UserException ex)
				{
					_logger.LogError(ex, "[Order] can't save - wrong upd num");
					TempData["ErrorMessage"] = ex.Message;
				}
				catch (DbUpdateException ex)
				{
					_logger.LogError(ex, string.Format(Const.ErrorMessages.CantSaveOrder, orderModel?.OrderId));

					if (!_orderManager.OrderExists(id))
					{
						return NotFound();
					}
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSave;
				}
				return RedirectToAction(nameof(Index));
			}
			return await ShowErrorOrder(orderModel, optOrder);
		}


		[HttpPost]
		public async Task DeletePhoto(string photoId)
		{
			if (string.IsNullOrWhiteSpace(photoId) ||
				!int.TryParse(photoId, out var id))
			{
				return;
			}

			await _orderManager.DeletePhotoAsync(id);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(string id)
		{
			if (!Guid.TryParse(id, out var guid))
				return BadRequest();

			try
			{
				await _orderManager.DeleteOrder(guid);
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteForUser;
				_logger.LogError(ex, Const.ErrorMessages.CantDeleteForUser);
			}
			return RedirectToAction(nameof(Index));
		}

		public async Task<ActionResult> Export(string id)
		{
			if (!Guid.TryParse(id, out var guid))
				return RedirectToAction(nameof(Index));
			var shortOrderId = await _orderManager.GetShortOrderId(guid);
			var user = await _userManager.GetUserAsync(User);
			var bytes = await _orderManager.Export(guid, user);
			return File(bytes, MediaTypeNames.Application.Octet, $"Заказ № {shortOrderId}.xlsx");
		}

		public void CheckOrderModel(OrderModel orderModel, OptOrder optOrder, bool isAdmin, bool isOperator)
		{
			if (orderModel.IsPaid != optOrder.IsPaid &&  /*только что поменяли*/
				!isAdmin)
			{
				ModelState.AddModelError(Const.ValidationMessages.ForbiddenIsPaidErrorKey, Const.ValidationMessages.ForbiddenIsPaidErrorMessage);
			}
			if (orderModel.DoneDate != optOrder.DoneDate &&
				orderModel.DoneDate?.Date < orderModel.OrderDate.Date)
			{
				ModelState.AddModelError(Const.ValidationMessages.DoneDateErrorKey, Const.ValidationMessages.DoneDateErrorMessage);
			}
			if (orderModel.AssemblyDate != optOrder.AssemblyDate &&
				orderModel.AssemblyDate?.Date < optOrder.OrderDate)
			{
				ModelState.AddModelError(Const.ValidationMessages.DateAssemblyErrorKey, Const.ValidationMessages.DateAssemblyErrorMessage);
			}

			if (orderModel.DoneDate != optOrder.DoneDate &&
				!isAdmin && !isOperator)
			{
				ModelState.AddModelError(Const.ValidationMessages.DoneDateChangeForbiddenErrorKey, Const.ValidationMessages.DoneDateChangeForbiddenErrorMessage);
			}

			if (orderModel.OrderStatusId == 3 /*Выполнен*/
				&& optOrder.OrderStatusId != orderModel.OrderStatusId /*только что поменяли*/)
			{
				if (!isAdmin && !isOperator)
				{
					ModelState.AddModelError(Const.ValidationMessages.StatusChangeErrorKey, Const.ValidationMessages.StatusChangeErrorMessage);
				}
			}
		}

		private async Task SetViewData(OrderModel model)
		{
			var clients = _userManager.Users.Select(t => new { t.Id, Name = t.LastName + " " + t.FirstName });
			ViewData["ClientId"] = new SelectList(clients, "Id", "Name", model.ClientId);
			ViewData["OrderStatusId"] = _orderManager.GetStatusList(model.OrderStatusId);
			ViewData["Action"] = "Edit";
			ViewData["OrderId"] = model.OrderId;
			ViewData["PersonalDiscountId"] = _orderManager.GetDiscountList();
		 
			ViewData["ClientType"] = model.ClientType;
			var user = await _userManager.GetUserAsync(User);
			ViewData["ShopId"] = _shopManager.GetClientShops(model.ClientId);
		}

		private async Task<OrderModel> GetOrderAsync(Guid orderId)
		{
			var user = await _userManager.GetUserAsync(User);
			var model = await _orderManager.GetOrderModelAsync(orderId, user);
			model.Promo = await GetGlobalSetting(Const.OrderPromoId);
			return model;
		}

		private async Task ChangeTrackNoNotify(OrderModel orderModel)
		{
			try
			{
				var client = await _userManager.FindByIdAsync(orderModel.ClientId);
				var shortOrderNum = orderModel.ShortOrderId > 0 ? orderModel.ShortOrderId.ToString() : Util.GetShortNum(orderModel.OrderId);
				_mailManager.SendNewTrackNoMail(shortOrderNum, orderModel.TrackNo, client.Email);
			}
			catch (Exception ex)
			{
				_logger.LogError("Ошибка при отправке оповещения пользователю о смене трек номера", ex);
			}
		}

		private async Task<IActionResult> ShowErrorOrder(OrderModel orderModel, OptOrder optOrder)
		{
			var onlyWithPrices = (await _userManager.GetUserAsync(User)).IsClient;
			await _orderManager.SetProducts(orderModel, optOrder, onlyWithPrices);
			_orderManager.SetPhotos(orderModel, optOrder);
			orderModel.OrderPayments = optOrder.OptOrderpayments?.ToList();
			ViewData["ClientId"] = new SelectList(_userManager.Users, "Id", "Id", orderModel.ClientId);
			ViewData["OrderStatusId"] = _orderManager.GetStatusList(orderModel.OrderStatusId);
			return View(orderModel);
		}
	}
}