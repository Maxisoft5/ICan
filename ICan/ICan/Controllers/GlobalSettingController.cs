using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ICan.Common.Models.Opt;
using ICan.Common;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,Operator")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class GlobalSettingController : BaseController
	{
		public GlobalSettingController(UserManager<ApplicationUser> userManager, GlobalSettingManager globalSettingManasger,
			ILogger<BaseController> logger) : base(userManager, logger)
		{
			_globalSettingManager = globalSettingManasger;
		}

		public async Task<IActionResult> Index()
		{
			var settings = await _globalSettingManager.Get();
			return View(settings);
		}

		public async Task<IActionResult> Edit(long id)
		{
			if (id == default(long))
			{
				return NotFound();
			}
			var setting = await _globalSettingManager.Get(id);
			if (setting == null)
			{
				return NotFound();
			}
			ViewData["Title"] = $"Настройка - {setting.Comment}";
			return View(setting);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("GlobalSettingId,Content")] GlobalSettingModel model)
		{
			if (id != model.GlobalSettingId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					await _globalSettingManager.Update(id, model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, string.Format(Const.ErrorMessages.CantSave, " глобальную настройку ", nameof(GlobalSettingController), nameof(Edit)));
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSave;
				}
				return RedirectToAction(nameof(Index));
			}
			return View(model);
		}
	}
}
