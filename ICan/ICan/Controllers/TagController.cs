
using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models.Enums;
using ICan.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ICan.Common.Models.Opt;
using ICan.Common;

namespace ICan.Controllers
{
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class TagController : BaseController
	{
		private readonly CommonManager<OptTag> _tagManager;

		public TagController(ILogger<BaseController> logger, CommonManager<OptTag> tagManager) : base(logger)
		{
			_tagManager = tagManager;
		}

		public async Task<IActionResult> Index()
		{
			var tags = await _tagManager.GetAsync<TagModel>();
			return View(tags);
		}

		public IActionResult Create()
		{
			ViewBag.Action = ActionType.Creation;
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(TagModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await _tagManager.AddAsync(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index));
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
			var model = await _tagManager.GetAsync<int, TagModel>(id);
			ViewBag.Action = ActionType.Edition;
			return View(nameof(Create), model);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(TagModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await _tagManager.UpdateAsync(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index));
				}
				catch (Exception ex)
				{
					_logger.LogError(ex.Message);
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				}
			}
			ViewBag.Action = ActionType.Edition;
			return View();
		}	 
	}
}
