using ICan.Business.Managers;
using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ICan.Controllers
{
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class GluePadIncomingController : BaseController
	{
		private readonly GluePadWarehouseManager _gluePadIncomingManager;

		public GluePadIncomingController(GluePadWarehouseManager gluePadIncomingManager, ILogger<BaseController> logger) : base(logger)
		{
			_gluePadIncomingManager = gluePadIncomingManager;
		}
		
		public async Task<IActionResult> Index()
		{
			var incomings = await _gluePadIncomingManager.GetIncomings();
			return View(incomings);
		}

		public IActionResult Create()
		{
			ViewBag.Action = ActionType.Creation;
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(GluePadIncomingModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await _gluePadIncomingManager.Create(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index));
				}
				catch(UserException ex)
				{
					_logger.LogError(ex.Message);
					TempData["ErrorMessage"] = ex.Message;
				}
				catch(Exception ex)
				{
					_logger.LogError(ex.Message);
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				}
			}
			ViewBag.Action = ActionType.Creation;
			return View();
		}


		public async Task<IActionResult> Edit(int id)
		{
			ViewBag.Action = ActionType.Edition;
			var incoming = await _gluePadIncomingManager.Get(id);
			return View("Create", incoming);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(GluePadIncomingModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await _gluePadIncomingManager.Update(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index));
				}
				catch (UserException ex)
				{
					_logger.LogError(ex.Message);
					TempData["ErrorMessage"] = ex.Message;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex.Message);
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				}
			}
			ViewBag.Action = ActionType.Edition;
			return View("Create", model);
		}

		public async Task<IActionResult> Delete(int id)
		{
			await _gluePadIncomingManager.DeleteIncoming(id);
			TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			return RedirectToAction(nameof(Index));
		}
	}
}
