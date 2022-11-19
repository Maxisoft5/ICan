using AutoMapper;
using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Domain;
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
	[Authorize(Roles = "Admin")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class RequisitesController : BaseController
	{
		private readonly CommonManager<OptRequisites> _commonManager;

		public RequisitesController(
			IMapper mapper,
			UserManager<ApplicationUser> userManager,
			ILogger<BaseController> logger,
			CommonManager<OptRequisites> commonManager)
			: base(mapper, userManager, logger)
		{
			_commonManager = commonManager;
		}

		public async Task<IActionResult> Index()
		{
			var requisites = await _commonManager.GetAsync();
			var list = _mapper.Map<IEnumerable<RequisitesModel>>(requisites);
			return View(list);
		}

		public IActionResult Create()
		{
			ViewData["ActionType"] = ActionType.Creation;
			var model = new RequisitesModel();
			return View("Edit", model);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("RequisitesId,Owner,RequisitesText,ClientType")] RequisitesModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					var raw = _mapper.Map<OptRequisites>(model);
					await _commonManager.AddAsync(raw);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, string.Format(Const.ErrorMessages.CantSave, " с реквизитами", nameof(RequisitesController), nameof(Edit)));
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				}
				return RedirectToAction(nameof(Index));
			}
			ViewData["ActionType"] = ActionType.Creation;
			return View("Edit", model);
		}

		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			ViewData["ActionType"] = ActionType.Edition;

			var raw = await _commonManager.GetAsync(id);

			if (raw == null)
			{
				return NotFound();
			}
			var model = _mapper.Map<RequisitesModel>(raw);
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("RequisitesId,Owner,RequisitesText,ClientType")] RequisitesModel model)
		{
			if (id != model.RequisitesId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var raw = await _commonManager.GetAsync(model.RequisitesId);
					raw.ClientType = (int)model.ClientType;
					raw.Owner = model.Owner;
					raw.RequisitesText = model.RequisitesText;
					await _commonManager.UpdateAsync(raw);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, string.Format(Const.ErrorMessages.CantSave, " с реквизитами", nameof(RequisitesController), nameof(Edit)));
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				}
				return RedirectToAction(nameof(Index));
			}
			return View(model);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _commonManager.DeleteAsync(id);
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, string.Format(Const.ErrorMessages.CantDelete, id));
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteForUser;
			}
			return RedirectToAction(nameof(Index));
		}
	}
}