using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ICan.Common.Models.Opt;
using ICan.Common;
using Microsoft.EntityFrameworkCore;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,Operator,Assembler")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class FormatController : BaseController
	{
		private readonly FormatManager _formatManager;

		public FormatController(
			UserManager<ApplicationUser> userManager,
			FormatManager formatManager,
			ILogger<BaseController> logger) : base(userManager, logger)
		{
			_formatManager = formatManager;
		}

		public async Task<IActionResult> Index()
		{
			var list = await _formatManager.GetAsync();
			return View(list);
		}

		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var format = await _formatManager.GetAsync(id);
			if (format == null)
			{
				return NotFound();
			}

			ViewData["ActionType"] = ActionType.Details;
			return View("Edit", format);
		}

		public IActionResult Create()
		{
			ViewBag.Action = ActionType.Creation;
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Name,Description")]
				FormatModel format)
		{
			if (ModelState.IsValid)
			{
				await _formatManager.AddAsync(format);
				return RedirectToAction(nameof(Index));
			}
			return View(format);
		}

		public async Task<IActionResult> Edit(int? id)
		{
			if (!id.HasValue)
			{
				return NotFound();
			}

			ViewData["ActionType"] = ActionType.Edition;
			var format = await _formatManager.GetAsync<int, FormatModel>(id.Value);
			if (format == null)
			{
				return NotFound();
			}
			return View("Edit", format);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("FormatId,Name,Description")] FormatModel format)
		{
			if (id != format.FormatId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					await _formatManager.UpdateAsync(format);
					return RedirectToAction(nameof(Index));
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, string.Format(Const.ErrorMessages.CantSave,
						"Edit", this.GetType().ToString(), format.FormatId));
				}
			}
			ViewData["ActionType"] = ActionType.Edition;
			return View(format);
		}


		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _formatManager.DeleteAsync(id);
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;

			}
			catch (DbUpdateException ex)
			{
				_logger.LogError(ex.Message);
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteFormat;
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteForUser;
				_logger.LogError(ex, Const.ErrorMessages.CantDeleteForUser);
			}

			return RedirectToAction(nameof(Index));
		}
	}
}
