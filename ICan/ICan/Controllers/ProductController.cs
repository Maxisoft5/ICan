using AutoMapper;
using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ICan.Common.Models.Opt;
using ICan.Common;
using Microsoft.AspNetCore.Http;

namespace ICan.Controllers
{
	[Authorize()]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class ProductController : BaseController
	{
		private readonly ProductManager _productManager;

		private readonly OrderManager _orderManager;

		private readonly PriceManager _priceManager;

		private readonly OrderMailManager _orderMailManager;

		public ProductController(IMapper mapper, ProductManager productManager,
			OrderManager orderManage, PriceManager priceManager,
			UserManager<ApplicationUser> userManager,
			ILogger<BaseController> logger,
			OrderMailManager mailManager,
			GlobalSettingManager globalSettingManager,
			IConfiguration configuration) : base(mapper, userManager, logger, configuration)
		{
			_productManager = productManager;
			_orderManager = orderManage;
			_priceManager = priceManager;
			_orderMailManager = mailManager;
			_globalSettingManager = globalSettingManager;
		}

		[Route("/opt")]
		[Route("/opt/Product")]
		[Route("/opt/Product/Index")]
		public async Task<IActionResult> Index()
		{
			var clientType = (int)ClientType.Unknown;
			ApplicationUser user = null;
			ProductListModel model = null;
			string clientId = null;
			if (User.Identity.IsAuthenticated)
			{
				try
				{
					user = await _userManager.GetUserAsync(User);
					var userRoles = await _userManager.GetRolesAsync(user);

					if (userRoles.Contains(Const.Roles.StoreKeeper))
						return RedirectToAction("Index", "Order");

					if (userRoles.Contains(Const.Roles.Designer))
						return RedirectToAction("Index", "PrintOrder");

					if (userRoles.Contains(Const.Roles.ContentMan) && !userRoles.Contains(Const.Roles.Operator))
						return RedirectToAction("ProductList", "SiteHome");

					if (userRoles.Contains(Const.Roles.Assembler))
						return RedirectToAction("Index", "SemiproductWarehouse");

					clientId = user.Id;
					var onlyWithPrices = !userRoles.Contains(Const.Roles.Admin)
									 && !userRoles.Contains(Const.Roles.Operator);

					clientType = user.ClientType;

					model = await _productManager.GetProductListModel(onlyWithPrices, user, dontShowDisabled: onlyWithPrices);
					model.Promo = await GetGlobalSetting(Const.ProductPromoId);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "При получении типа клиента произошла ошибка");
				}
			}
			ViewData["ClientType"] = clientType;

			return View(model);
		}

		public async Task<IActionResult> List()
		{

			try
			{
				IEnumerable<ProductModel> list = await _productManager.GetProductPlainListModel();
				return Ok(list.ToDictionary(item => item.ProductId, item=>item.DisplayName ));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[Product][List]");
			}

			return BadRequest();
		}

		[HttpGet]
		public async Task<IActionResult> GetModel(string clientId)
		{
			var model = await GetModel(true, clientId);
			return PartialView("_ProductList", model);
		}


		[HttpPost]
		[Authorize()]
		public async Task<IActionResult> Post([FromBody] ClientOrderModel clientOrder)
		{
			var clientId = clientOrder.ClientId ?? _userManager.GetUserId(User);
			var client = await _userManager.FindByIdAsync(clientId);
			clientOrder.Address = System.Net.WebUtility.HtmlEncode(clientOrder.Address);
			clientOrder.PvzAddress = System.Net.WebUtility.HtmlEncode(clientOrder.PvzAddress);

			var raw = await _orderManager.CreateOrderAsync(clientId, clientOrder);

			_orderMailManager.SendNewOrderMail(raw.OrderId, client.Email);
			return new JsonResult(new { raw.OrderId });
		}

		[HttpPost]
		[Authorize()]
		public async Task<IActionResult> Update([FromBody] ClientOrderModel clientOrder)
		{
			Guid.TryParse(clientOrder.OrderId, out var orderGuid);
			var orderNum = await _orderManager.UpdateOrder(orderGuid, clientOrder.OrderItems.Where(t => t.Amount > 0));
			return new JsonResult(new { orderNum });
		}

		[Authorize(Roles = "Admin,Operator")]
		public async Task<IActionResult> Create()
		{
			await SetProductSeriesViewData();
			return View();
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> AddKitProduct(KitProductModel model)
		{
			if (ModelState.IsValid)
			{
				await _productManager.AddKitProductAsync(model);
				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
			}
			else
			{
				TempData["ErrorMessage"] = GetErrors();
			}
			return RedirectToAction(nameof(Edit), new { id = model.MainProductId });
		}


		[HttpPost]
		[Authorize(Roles = "Admin,Operator")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("ProductId,Name,ProductKindId,IsKit,Enabled,ProductSeriesId,ProductUrl,Weight,ISBN,DisplayOrder,CountryId,RegionalName,ArticleNumber,ShowPreviousPrice")] ProductModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					await _productManager.CreateAsync(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index));
				}
			}
			catch (UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				_logger.LogError(ex, "[Produt] Возникла ошибка при создании тетради");
			}

			await SetProductSeriesViewData();
			return View(model);
		}


		public async Task<IActionResult> Details(int? id)
		{
			var model = _productManager.GetDetails(id);
			if (model == null)
				return NotFound();

			SetProductPricesAndKitProducts(model);
			await SetViewData(model);
			ViewData["ActionType"] = ActionType.Details;
			return View("Edit", model);
		}

		[Authorize(Roles = "Admin,Operator")]
		public async Task<IActionResult> Edit(int? id)
		{
			var model = _productManager.GetDetails(id);
			if (model == null)
				return NotFound();

			SetProductPricesAndKitProducts(model);
			await SetViewData(model);
			ViewData["ActionType"] = ActionType.Edition;
			return View(model);
		}


		[HttpPost]
		[Authorize(Roles = "Admin,Operator")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id,
			[Bind("ProductId,Name,ProductKindId,IsKit,Enabled,ProductSeriesId,ProductUrl,Weight,ISBN,DisplayOrder,CountryId,RegionalName,,ArticleNumber,ShowPreviousPrice")] ProductModel model)
		{
			if (id != model.ProductId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					await _productManager.Edit(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index));
				}
				catch (UserException ex)
				{
					TempData["ErrorMessage"] = ex.Message;
				}
				catch (Exception ex)
				{
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
					_logger.LogError(ex, string.Format(Const.ErrorMessages.CantSaveProduct, model?.ProductId));
				}
			}
			SetProductPricesAndKitProducts(model);
			await SetViewData(model);
			ViewData["ActionType"] = ActionType.Edition;
			return View(model);
		}

		[Authorize(Roles = "Admin,Operator")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeletePrice(int id)
		{
			var productPrice = await _priceManager.GetAsync(id);
			IEnumerable<ProductpriceModel> productPrices = Enumerable.Empty<ProductpriceModel>();
			if (productPrice != null)
			{
				try
				{
					await _priceManager.RemoveAsync(id);
					TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
				}
				catch (DbUpdateException dbEx)
				{
					var warnString = "Невозможно удалить цену, так как она используется в заказах";
					TempData["ErrorMessage"] = warnString;
					_logger.LogWarning(dbEx, $"[Цена][Удаление] {warnString}. Id {id},");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"[Цена][Удаление] Возникла ошибка. Id {id}");
				}
				productPrices = _priceManager.GetPrices(productPrice.ProductId);

				ViewData["ProductId"] = productPrice.ProductId;
				ViewData["ActionType"] = ActionType.Edition;
				return PartialView("_ProductPrice", productPrices);

			}
			TempData["ErrorMessage"] = "Не найдена цена. Обновите страницу";
			return NotFound();
		}


		[Authorize(Roles = "Admin,Operator")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteKitProduct(int id)
		{
			try
			{
				await _productManager.DeleteKitProduct(id);
				return Ok();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"[Product] Kit product with id {id} wasn't found");
				return BadRequest("Невозможно удалить компонент");
			}
		}

		[Authorize(Roles = "Admin,Operator")]
		public IActionResult KitProducts(int id)
		{
			try
			{
				var kitProducts = _productManager.GetKitProduct(id);
				return PartialView("_KitProducts", kitProducts);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"[Product] Kit products for main product with id {id} were not found");
				return BadRequest("Возникла ошибка");
			}
		}

		[Authorize(Roles = "Admin,Operator")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _productManager.Delete(id);
				TempData["SuccessMessage"] = Const.SuccessMessages.Deleted;
			}
			catch (UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (Exception ex)
			{
				var errorMessage = string.Format(Const.ErrorMessages.CantDeleteProduct, id);
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteForUser;
				_logger.LogError(ex, errorMessage);
			}
			return RedirectToAction(nameof(Index));
		}

		[Authorize(Roles = "Admin,Operator")]
		[HttpPost]
		public async Task<IActionResult> AddPrice(int id, double price)
		{
			await _priceManager.AddPrice(id, price);

			ViewData["ProductId"] = id;
			List<ProductpriceModel> productPrices = _priceManager.GetPrices(id);
			ViewData["ActionType"] = ActionType.Edition;
			return PartialView("_ProductPrice", productPrices);
		}

		[Authorize(Roles = "Admin,Operator")]
		[HttpPost()]
		public async Task<IActionResult> ImportPrices(string id, IFormFile file)
		{
			if (file == null || file.Length == 0 || !file.FileName.Contains(".xlsx"))
				return BadRequest();

			try
			{
				await _productManager.ParsePriceFile(file);
				return Ok();
			}
			catch (Exception ex)
			{
				_logger.LogError("не удалось импортировать цены", ex);
			}
			return BadRequest();
		}


		private async Task<ProductListModel> GetModel(bool onlyWithPrices, string clientId)
		{
			var user = _userManager.Users.First(us => us.Id == clientId);
			var model = await _productManager.GetProductListModel(onlyWithPrices, user);
			model.Promo = await GetGlobalSetting(Const.ProductPromoId);
			return model;
		}

		private async Task SetProductSeriesViewData()
		{
			ViewData["ProductSeriesId"] = new SelectList(await _productManager.GetProductSeries(), "ProductSeriesId", "Name");
			ViewData["CountryId"] = new SelectList(await _productManager.GetCountries(), "CountryId", "Name");
		}

		private void SetProductPricesAndKitProducts(ProductModel model)
		{
			List<ProductpriceModel> productPrices = _priceManager.GetPrices(model.ProductId);
			model.ProductPrices = productPrices;
			if (model.IsKit)
			{
				model.KitProducts = _productManager.GetKitProducts(model.ProductId);
			}
		}

		private async Task SetViewData(ProductModel model)
		{
			await SetProductSeriesViewData();
			ViewData["ProductId"] = model.ProductId;
			ViewData["NotebookProductId"] = GetNotebooks(model);
		}

		private IEnumerable<SelectListItem> GetNotebooks(ProductModel model)
		{
			var existingItems = _productManager.GetExistingItems(model.ProductId);

			var notebooks = _productManager.GetNotebooksExcludeExistings(existingItems, model.ProductSeriesId);

			return notebooks.Select(notebook => new SelectListItem { Text = notebook.DisplayName, Value = notebook.ProductId.ToString() });
		}
	}
}
