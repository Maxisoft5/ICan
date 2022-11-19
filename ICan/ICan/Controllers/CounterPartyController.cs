using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ICan.Common.Models.Opt;
using ICan.Common;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,Assembler")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class CounterPartyController : BaseController
	{
		private CounterPartyManager _counterPartyManager;

		public CounterPartyController(CounterPartyManager counterPartyManager, UserManager<ApplicationUser> userManager,
			ILogger<BaseController> logger) : base(userManager, logger)
		{
			_counterPartyManager = counterPartyManager;
		}

		public IActionResult Index()
		{
			IEnumerable<CounterpartyModel> modelList = _counterPartyManager.Get();
			return View(modelList);
		}

		public IActionResult Create()
		{
			ViewBag.Action = ActionType.Creation;
			SetViewData();
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Name,Consignee,Inn,Enabled,PaperOrderRoleId,PaymentDelay")]
				CounterpartyModel model)
		{
			if (ModelState.IsValid)
			{
				await _counterPartyManager.Add(model);
				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				return RedirectToAction(nameof(Index));
			}
			SetViewData();
			return View(model);
		}

		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}
			CounterpartyModel model = await _counterPartyManager.Get(id);
			if (model == null)
			{
				return NotFound();
			}

			SetViewData();
			return View("Edit", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id,
		   [Bind("CounterpartyId,Name,Consignee,Inn,Enabled,PaperOrderRoleId,PaymentDelay")]
			CounterpartyModel model)
		{
			if (id != model.CounterpartyId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					await _counterPartyManager.Update(id, model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index));
				}
				catch (Exception ex)
				{
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
					_logger.LogError(ex, string.Format(Const.ErrorMessages.CantSave,
						"Edit", this.GetType().ToString(), model.CounterpartyId));
				}
			}
			SetViewData();
			return View(model);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _counterPartyManager.Delete(id);
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			}
			catch (Exception ex)
			{
				var errorString = Const.ErrorMessages.CantDeleteForUser;
				TempData["ErrorMessage"] = errorString;
				_logger.LogError(ex, errorString);
			}
			return RedirectToAction(nameof(Index));
		}

		private void SetViewData()
		{
			ViewData["PaperOrderRole"] = _counterPartyManager.GetPaperOrderRoles();
		}
	}
}
