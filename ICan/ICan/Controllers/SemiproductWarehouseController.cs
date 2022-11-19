using AutoMapper;
using ICan.Business.Managers;
using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,Assembler")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class SemiproductWarehouseController : BaseController
	{
		private readonly WarehouseManager _warehouseManager;
		private readonly SemiproductWarehouseManager _semiproductWarehouseManager;
		private readonly CalcManager _calcManager;
		private readonly ProductManager _productManager;
		private readonly SemiproductManager _semiproductManager;
		private readonly CommonManager<OptProductseries> _productSeriesManager;

		public SemiproductWarehouseController(IMapper mapper,
			UserManager<ApplicationUser> userManager,
			ProductManager productManager,
			SemiproductWarehouseManager semiproductWarehouseManager,
			CalcManager calcManager,
			ILogger<BaseController> logger,
			SemiproductManager semiproductManager,
			CommonManager<OptProductseries> productSeriesManager,
			WarehouseManager warehouseManager) : base(mapper, userManager, logger)
		{
			_semiproductWarehouseManager = semiproductWarehouseManager;
			_warehouseManager = warehouseManager;
			_calcManager = calcManager;
			_productManager = productManager;
			_semiproductManager = semiproductManager;
			_productSeriesManager = productSeriesManager;
		}

		public IActionResult Index()
		{
			var list = _semiproductWarehouseManager.GetWarehouseList();
			return View(list);
		}

		public IActionResult Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var model = _semiproductWarehouseManager.GetWarehouse(id);

			SetViewData(model.WarehouseTypeId);
			return View(model);
		}

		public IActionResult Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var model = _semiproductWarehouseManager.GetWarehouse(id);

			SetViewData(model.WarehouseTypeId);
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(SemiproductWarehouseModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					await _semiproductWarehouseManager.Update(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index));
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Возникла ошибка при редактировании записи в склад полуфабрикатов");
			}
			await SetModelItems(model);
			TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> CalculateWhJournal()
		{
			var model = await _semiproductWarehouseManager.CalculateWhJournalSemiproducts();
			return View("SemiproductsWarehouseJournalState", model);
		}

		[HttpGet]
		public async Task<IActionResult> SemiproductState(int productId)
		{
			var model = await _semiproductWarehouseManager.CalcSemiproductWhjournalAsync(productId);
			ViewData["ProductName"] = model.Key;
			return View("SemiproductState", model.Value);
		}


		[HttpGet(Name = "Report")]
		public async Task<IActionResult> SemiproductReportAsync()
		{
			var bytes = await _semiproductWarehouseManager.GetReportAsync();
			return File(bytes, MediaTypeNames.Application.Octet, $"Отчёт по состоянию полуфабрикатов {DateTime.Now}.xlsx");
		}

		public IActionResult Create()
		{
			var model = new SemiproductWarehouseModel
			{
				WarehouseActionTypeId = (int)WarehouseActionType.Inventory,
				SemiproductWarehouseFullItems = _semiproductWarehouseManager.GetSemiproductsFullList(),
				Date = DateTime.Now,
			};
			SetViewData();
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(SemiproductWarehouseModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					await _semiproductWarehouseManager.AddInventoryAsync(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index));
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Возникла ошибка при добавлении записи в склад полуфабрикатов");
			}
			await SetModelItems(model);
			TempData["ErrorMessage"] = Const.ErrorMessages.CheckNumbers;
			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> PartialInventory()
		{
			var model = new PartialInventoryModel
			{
				SemiproductList = await _semiproductManager.GetSemiproductList(),
			};
			SetViewData();
			await SetProductSeriesList();
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> PartialInventory(PartialInventoryModel model)
		{
			try
			{
				await _semiproductWarehouseManager.AddPartial(model);
				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Возникла ошибка при добавлении записи в склад полуфабрикатов");
			}
			TempData["ErrorMessage"] = Const.ErrorMessages.CheckNumbers;
			return RedirectToAction("PartialInventory");
		}

		public IActionResult GetSemiproductsByProductId(int productId)
		{
			var semiprods = _semiproductWarehouseManager.GetSemiprudctsForPartialInventory(productId);
			return Ok(semiprods.Select(x => new { x.DisplayName, x.SemiproductId }));
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _warehouseManager.Delete(id, User.IsInRole(Const.Roles.Admin), User.IsInRole(Const.Roles.Assembler));
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			}
			catch (UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"[SemiproductWarehouse] {Const.ErrorMessages.CantDeleteForUser}");
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteForUser;
			}
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> GetSemiproductsBySeries(int productSeriesId, int whType)
		{
			var semiprods = await _semiproductWarehouseManager.GetSemiproductsForInventory(productSeriesId, whType);
			return Ok(semiprods.Select(x => new { Text = x.DisplayName, Value = x.SemiproductId }));
		}

		private async Task<IEnumerable<SemiproductWarehouseItemModel>> GetSemiproducts()
		{
			var semiproductList = await _semiproductManager.GetSemiproductList();
			return semiproductList.Select(smProduct =>
				new SemiproductWarehouseItemModel
				{
					SemiProductId = smProduct.SemiproductId,
					SemiproductName = smProduct.DisplayName
				});
		}


		private void SetViewData(int selected = (int)WarehouseType.SemiproductReady)
		{
			ViewData["WarehouseActionTypeId"] = _calcManager.GetActions(availableActions: new List<int> { (int)WarehouseActionType.Inventory });
			ViewData["WarehouseTypeId"] =
			_warehouseManager.GetWarehousesTypesByWhObjectType(WhJournalObjectType.Semiproduct, (WarehouseType)selected);
		}

		private async Task SetProductSeriesList()
		{
			var series = await _productSeriesManager.GetAsync<ProductSeriesModel>();
			var seriesList = series.Where(x => x.ProductKindId == (int)ProductKind.Notebook)
						.Select(series => new SelectListItem { Text = series.Name, Value = series.ProductSeriesId.ToString() });
			ViewData["ProductSeries"] = seriesList;
		}

		private async Task SetModelItems(SemiproductWarehouseModel model)
		{
			var semiproducts = await GetSemiproducts();
			semiproducts.ToList().ForEach(semiproduct =>
			{
				var modelItem = model.SemiproductWarehouseItems.FirstOrDefault(mItem => mItem.SemiProductId == semiproduct.SemiProductId && mItem.Amount > 0);
				if (modelItem != null)
				{
					semiproduct.Amount = modelItem.Amount;
				}
			});
			model.SemiproductWarehouseItems = semiproducts;
		}
	}
}
