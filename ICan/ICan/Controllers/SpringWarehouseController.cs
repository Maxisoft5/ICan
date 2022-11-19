using ICan.Business.Managers;
using ICan.Common;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Opt;
using ICan.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Controllers
{
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class SpringWarehouseController : BaseController
	{
		private readonly SpringManager _springManager;
		private readonly UniversalWarehouseManager _universalWarehouseManager;

		public SpringWarehouseController(
			ILogger<BaseController> logger,
			SpringManager springManager,
			UniversalWarehouseManager universalWarehouseManager)
			: base(logger)
		{
			_springManager = springManager;
			_universalWarehouseManager = universalWarehouseManager;
		}

		public async Task<IActionResult> Index()
		{
			var springs = await _springManager.GetSprings();
			var springIds = springs.Select(sp => (long)sp.SpringId);
			var warehouseItems = await _universalWarehouseManager.GetWarehouseInfo(WarehouseType.Spings, WhJournalObjectType.Spring, springIds);
			ViewData["Title"] = "Склад пружин";
			ViewData["WarehouseType"] = WarehouseType.Spings;
			return View("~/Views/UniversalWarehouse/Index.cshtml", warehouseItems);
		}

		public async Task<IActionResult> Inventory()
		{
			var springs = await _springManager.GetSprings();
			var model = new WarehouseModel
			{
				WarehouseTypeId = WarehouseType.Spings,
				WarehouseItems = springs.Select(sp => new WarehouseItemModel
				{
					ObjectDisplayName = sp.SpringName,
					ObjectId = sp.SpringId,
				}).ToList(),
			};
			return View("~/Views/UniversalWarehouse/Inventory.cshtml", model);
		}

		public async Task<IActionResult> AddInventory(WarehouseModel model)
		{
			await _universalWarehouseManager.AddInventory(model);
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Details(int id)
		{
			var details = await _universalWarehouseManager.GetDetails(id, WhJournalObjectType.Spring);
			return View("~/Views/UniversalWarehouse/Details.cshtml", details);
		}


		public async Task<IActionResult> State()
		{
			ViewData["Title"] = "Состояние склада пружин";
			var details = await _universalWarehouseManager.GetState(WarehouseType.Spings);
			return View("~/Views/UniversalWarehouse/State.cshtml", details);
		}

		public async Task<IActionResult> Delete(int id)
		{
			await _universalWarehouseManager.Delete(id);
			return RedirectToAction(nameof(Index));
		}
	}
}
