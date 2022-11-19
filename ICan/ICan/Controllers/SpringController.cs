using AutoMapper;
using ICan.Business.Managers;
using ICan.Common;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using ICan.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,Assembler")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class SpringController : BaseController
	{
		private readonly SpringManager _springManager;


		public SpringController(ILogger<BaseController> logger, SpringManager springManager)
			: base(logger)
		{
			_springManager = springManager;
		}

		public async Task<IActionResult> Index()
		{
			var springs = await _springManager.GetSprings();
			return View(springs);
		}

		[HttpGet]
		public async Task<IActionResult> Create()
		{
			await SetNumberOfTurnsList();
			ViewBag.Action = ActionType.Creation;
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(SpringModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await _springManager.Create(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index));
				}
				catch (Exception ex)
				{
					_logger.LogError(ex.Message);
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSave;
					ViewBag.Action = ActionType.Creation;
				}
			}
			await SetNumberOfTurnsList();
			return View();
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var model = await _springManager.GetById(id);
			await SetNumberOfTurnsList();
			ViewBag.Action = ActionType.Edition;
			return View("Create", model);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(SpringModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await _springManager.Edit(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index));
				}
				catch (Exception ex)
				{
					_logger.LogError(ex.Message);
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSave;
					ViewBag.Action = ActionType.Edition;
				}
			}
			await SetNumberOfTurnsList();
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _springManager.Delete(id);
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			}
			catch (UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (DbUpdateException ex)
			{
				_logger.LogError(ex.Message);
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteSpring;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteForUser;
			}
			return RedirectToAction(nameof(Index));
		}

		private async Task SetNumberOfTurnsList()
		{
			var numberOfTurnsList = await _springManager.GetNumberOfTurns();
			ViewData["NumberOfTurnsList"] = numberOfTurnsList.Select(x => new SelectListItem(x.NumberOfTurns.ToString(), x.NumberOfTurnsId.ToString()));
		}
	}
}
