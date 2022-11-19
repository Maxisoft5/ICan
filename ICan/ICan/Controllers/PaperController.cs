using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ICan.Common.Models.Opt;
using ICan.Common;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,Assembler")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class PaperController : BaseController
	{
		private readonly CommonManager<OptPaper> _commonManager;
		private readonly TypeOfPaperManager _typeOfPaperManager;
		public PaperController(
			CommonManager<OptPaper> commonManager,
			UserManager<ApplicationUser> userManager,
			ILogger<BaseController> logger,
			TypeOfPaperManager typeOfPaperManager) : base(userManager, logger)
		{
			_commonManager = commonManager;
			_typeOfPaperManager = typeOfPaperManager;
		}

		public IActionResult Index()
		{
			var list = _commonManager.Get<PaperModel>("TypeOfPaper");
			return View(list);
		}

		public async Task<IActionResult> Create()
		{
			ViewBag.Action = ActionType.Creation;
			await SetTypesOfPaperList();
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Name,Description,TypeOfPaperId,Width,Length")]
				PaperModel paper)
		{
			if (ModelState.IsValid)
			{
				await _commonManager.AddAsync(paper);
				return RedirectToAction(nameof(Index));
			}
			await SetTypesOfPaperList();
			return View(paper);
		}

		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}
			ViewData["ActionType"] = ActionType.Edition;

			var paper = await _commonManager.GetAsync<int, PaperModel>(id.Value);
			if (paper == null)
			{
				return NotFound();
			}
			await SetTypesOfPaperList();		
			return View("Edit", paper);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id,
		  [Bind("PaperId,Name,Description,TypeOfPaperId,Width,Length")]
			PaperModel paper)
		{
			if (id != paper.PaperId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					await _commonManager.UpdateAsync(paper);
					return RedirectToAction(nameof(Index));
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, string.Format(Const.ErrorMessages.CantSave,
						"Edit", this.GetType().ToString(), paper.PaperId));
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				}
			}
			ViewData["ActionType"] = ActionType.Edition;
			await SetTypesOfPaperList();
			return View(paper);
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

		private async Task SetTypesOfPaperList()
		{
			var typesOfPaper = await _typeOfPaperManager.GetTypes();
			ViewData["TypesOfPaper"] = new SelectList(typesOfPaper, "TypeOfPaperId", "Name");
		}
	}
}
