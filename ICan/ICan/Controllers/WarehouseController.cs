using ICan.Business.Managers;
using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Extensions;
using ICan.Jobs.OneC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace ICan.Controllers
{
    [Authorize(Roles = "Admin,Operator,StoreKeeper")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class WarehouseController : BaseController
	{
		private readonly ProductManager _productManager;

		private readonly WarehouseManager _warehouseManager;
		private readonly CalcManager _calcManager;
		private readonly OneCImportJob _job;

		public WarehouseController(
			UserManager<ApplicationUser> userManager,
			CalcManager calcManager,
			ILogger<BaseController> logger,
			OneCImportJob job,
			ProductManager productManager,
			 WarehouseManager warehouseManager
			) : base(userManager, logger)
		{
			_job = job;
			_productManager = productManager;
			_warehouseManager = warehouseManager;
			_calcManager = calcManager;
		}

		public IActionResult Index() => View();

		public async Task<IActionResult> IndexData(TableOptions options)
		{
			var stopW = new Stopwatch();
			stopW.Start();
			
			var list = _warehouseManager.Get(WhJournalObjectType.Notebook, options,  out var total);
			stopW.Stop();
			_logger.LogWarning($"elapsed {stopW.Elapsed}");
			return Ok(new { total, rows = list });
		}

		public async Task<ActionResult> Details(int id)
		{
			var raw = await _warehouseManager.GetWarehouseDetailsAsync(id);

			return await GetDetailsView(raw, ActionType.Details);
		}

		[Authorize(Roles = "Admin")]
		public async Task<ActionResult> Edit(int id)
		{
			var raw = await _warehouseManager.GetWarehouseDetailsAsync(id);

			var model = await _warehouseManager.GetDetailsModelAsync(raw, ActionType.Edition);
			return View(model);
		}

		public async Task<IActionResult> Actions()
        {
			var list = _warehouseManager.GetActions();
			return Ok(list.ToDictionary(item => item.Value, item => item.Text));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult UploadFilesFromFTP()
		{
			_job.Import();
			return Ok();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> CalculateWhj()
		{
			WarehouseStateModel model = await _warehouseManager.GetWarehouseState();
			return View("WarehouseState", model);
		}

		[HttpGet]
		public async Task<ActionResult> ExportCalculateWhj()
		{
			byte[] bytes = await _warehouseManager.GetWhStateFile();
			return File(bytes, MediaTypeNames.Application.Octet);
		}

		[HttpGet]
		public async Task<IActionResult> ProductState(int productId)
		{
			var product = _productManager.GetDetails(productId);
			var inventory = await _calcManager.GetLatestInventoryAsync(WhJournalObjectType.Notebook);
			var inventoryAmount = inventory.WarehouseItems
				.FirstOrDefault(inv => inv.ProductId == productId)?.Amount ?? 0;


			var (journal, singleInventory) = _calcManager.GetProductWarehouseState(productId, inventory.DateAdd, WarehouseType.NotebookReady);

			var caldWjhDetails = new CalcWhjDetails
			{
				ProductId = productId,
				Name = product.DisplayName,
				IsKit = product.IsKit,
				AssemblesAsKit = product.AssemblesAsKit,
				ProductSeriesId = product.ProductSeriesId,
				Journal = journal,
				Inventory = inventoryAmount,
				InventoryDate = inventory?.DateAdd,
				SingleInventoryDate = singleInventory?.DateAdd,
				SingleInventory = singleInventory?.WarehouseItems.First(whItem => whItem.ProductId == product.ProductId).Amount,
			};

			return PartialView("ProductState", caldWjhDetails);
		}

		public async Task<ActionResult> Create()
		{
			var model = new WarehouseModel
			{
				DateAdd = DateTime.Now,
				WarehouseTypeId = WarehouseType.NotebookReady,
			};
			await _warehouseManager.SetListsAsync(model);
			return View("Create", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Create(WarehouseModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					if (model.WarehouseActionTypeId == (int)WarehouseActionType.Arrival)
					{
						_warehouseManager.CheckAssemblyIsAvailable(model);
					}

					await _warehouseManager.Create(model);

					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index), new { objectType = WhJournalObjectType.Notebook });
				}
				else
				{
					TempData["ErrorMessage"] = GetErrors();
				}
			}
			catch (UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				_logger.LogError(ex, null);
			}

			await _warehouseManager.SetListsAsync(model);
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult> Edit(WarehouseModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					await _warehouseManager.Update(model);
					TempData["StatusMessage"] = "Информация успешно сохранена";
					return RedirectToAction(nameof(Index), new { objectType = WhJournalObjectType.Notebook });
				}
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = "При сохраении произошла ошибка";
				_logger.LogError(ex, null);
			}

			model.WarehouseActionTypes = _warehouseManager.GetActions();

			return View(model);
		}

		[HttpPost]
		[Authorize(Roles = "Admin,StoreKeeper")]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _warehouseManager.Delete(id, User.IsInRole(Const.Roles.Admin));
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			}
			catch (DbUpdateException ex)
			{
				_logger.LogError(ex, ex.GetType().ToString());
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteForUser;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.GetType().ToString());
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteForUser;
			}
			return RedirectToAction(nameof(Index), new { objectType = WhJournalObjectType.Notebook });
		}

		private async Task<ActionResult> GetDetailsView(OptWarehouse raw, ActionType actionType)
		{
			var model = await _warehouseManager.GetDetailsModelAsync(raw, actionType);
			return View("Details", model);
		}
	}
}