using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ICan.Common.Models.Opt;
using ICan.Common;
using System.Linq;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,Assembler")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class PaperWarehouseController : BaseController
	{
		private readonly PaperWarehouseManager _warehouseManager;
		private readonly CommonManager<OptPaper> _paperManager;
		private readonly CalcManager _calcManager;

		public PaperWarehouseController(ILogger<PaperWarehouseController> logger, PaperWarehouseManager warehouseManager, CalcManager calcManager,
			CommonManager<OptPaper> paperManager) : base(logger)
		{
			_warehouseManager = warehouseManager;
			_calcManager = calcManager;
			_paperManager = paperManager;
		}

		public async Task<IActionResult> Index()
		{
			var warehouseItems = await _warehouseManager.Get(WhJournalObjectType.Paper);
			return View(warehouseItems);
		}

		public async Task<IActionResult> Create()
		{
			var model = new WarehouseModel
			{
				DateAdd = DateTime.Now,
				WarehouseTypeId = WarehouseType.PaperReady
			};
			await _warehouseManager.SetListsAsync(model);
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(WarehouseModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					await _warehouseManager.Create(model);
				}
			}
			catch (UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = "При сохраении произошла ошибка";
				_logger.LogError(ex, null);
			}

			return RedirectToAction(nameof(Index));
		}

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Edit(int id)
		{
			var warehouse = await _warehouseManager.GetWarehouseDetailsAsync(id);

			var model = await _warehouseManager.GetDetailsModelAsync(warehouse, ActionType.Edition);
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(WarehouseModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					await _warehouseManager.Update(model);
					TempData["StatusMessage"] = "Информация успешно сохранена";
					return RedirectToAction(nameof(Index), new RouteValueDictionary(new { objectType = WhJournalObjectType.Paper }));
				}
			}
			catch (UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = "При сохраении произошла ошибка";
				_logger.LogError(ex, null);
			}

			model.WarehouseActionTypes = _warehouseManager.GetActions();

			return View(model);
		}

		[Authorize(Roles = "Admin")]
		[HttpPost]
		[ValidateAntiForgeryToken]
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

			return RedirectToAction(nameof(Index), new { objectType = WhJournalObjectType.Paper });
		}

		public async Task<ActionResult> Details(int id)
		{
			var raw = await _warehouseManager.GetWarehouseDetailsAsync(id);
			var details = await _warehouseManager.GetDetailsModelAsync(raw, ActionType.Details);

			return View(details);
		}

		[HttpGet]
		public async Task<IActionResult> PaperWarehouseState(int id)
		{
			var paperWarhouses = _warehouseManager.GetWarehouseTypes();
			var modelList = new List<CalcPaperWhjDetails>();
			foreach (var warehouse in paperWarhouses)
			{
				var model = await _calcManager.CalculatePaperFromWhjAsync((WarehouseType)warehouse.WarehouseTypeId,
					new List<long> { id });
				modelList.AddRange(model);
			}

			return PartialView("PaperItemWarehouseState", modelList);
		}

		[HttpPost]
		public async Task<ActionResult> CalculateWhj()
		{
			var paperWarhouses = _warehouseManager.GetWarehouseTypes();
			var papers = await _paperManager.GetAsync<PaperModel>();
			var whState = new List<CalcPaperWhjDetails>();
			foreach (var paperWH in paperWarhouses)
			{
				var state = await _calcManager.CalculatePaperFromWhjAsync((WarehouseType)paperWH.WarehouseTypeId, papers.Select(p => (long)p.PaperId));
				whState.AddRange(state);
			}

			var model = whState.GroupBy(item => item.PaperId)
				.Select(group =>
				new PaperWarehouseDetail
				{
					PaperId = group.Key,
					Name = group.FirstOrDefault()?.PaperName,
					Warehouses = group.ToList(),
				}
			);
			ViewData["Warehouses"] = paperWarhouses;

			return View("PaperWarehouseState", model);
		}
	}
}
