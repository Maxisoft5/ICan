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
	public class GluePadWarehouseController : BaseController
	{
		private readonly GluePadWarehouseManager _gluePadWarehouseManager;
		private readonly UniversalWarehouseManager _universalWarehouseManager;

		public GluePadWarehouseController(
			ILogger<BaseController> logger,
			GluePadWarehouseManager gluePadWarehouseManager,
			UniversalWarehouseManager universalWarehouseManager)
			: base(logger)
		{
			_gluePadWarehouseManager = gluePadWarehouseManager;
			_universalWarehouseManager = universalWarehouseManager;
		}

		public async Task<IActionResult> Index()
		{
			var products = new List<long> { Const.GluePadProductId };
			ViewData["Title"] = "Склад клеевых подушек";
			var items = await _universalWarehouseManager.GetWarehouseInfo(WarehouseType.GluePads, WhJournalObjectType.GluePad, products);
			/*ViewData["AdditionalInfo"] = $"Клеевых подушек на складе: {items.Current}"*/;
			ViewData["WarehouseType"] = WarehouseType.GluePads;
			return View("~/Views/UniversalWarehouse/Index.cshtml", items);
		}

		public async Task<IActionResult> State()
		{
			ViewData["Title"] = "Состояние склада клеевых подушек";
			var details = (await _universalWarehouseManager.GetState(WarehouseType.GluePads)).First();
			return View(details);
		}

		public IActionResult Inventory()
		{
			return View();
		}

		public async Task<IActionResult> AddInventory(int amount, string comment)
		{
			var wh = new WarehouseModel
			{
				Comment = comment,
				WarehouseTypeId = WarehouseType.GluePads,
				WarehouseItems = new List<WarehouseItemModel>
					{
						new WarehouseItemModel
						{
							Amount = amount,
							ObjectId = Const.GluePadProductId,
						},
					},
			};
			await _universalWarehouseManager.AddInventory(wh);
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Details(int id)
		{
			var details = await _universalWarehouseManager.GetDetails(id, WhJournalObjectType.GluePad);
			return View("~/Views/UniversalWarehouse/Details.cshtml", details);
		}

		public async Task<IActionResult> Delete(int id)
		{
			await _universalWarehouseManager.Delete(id);
			return RedirectToAction(nameof(Index));
		}
	}
}
