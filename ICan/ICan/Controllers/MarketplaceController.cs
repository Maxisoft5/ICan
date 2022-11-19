using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models.Exceptions;
using ICan.Common.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ICan.Common.Models.Opt;
using ICan.Common;

namespace ICan.Controllers
{
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class MarketplaceController : BaseController
	{
		private readonly MarketplaceManager _marketplaceManager;

		/// <summary>
		/// Initializes a new instance of the <see cref="MarketplaceController"/> class.
		/// </summary>
		/// <param name="marketplaceManager"></param>
		/// <param name="logger"></param>
		public MarketplaceController(MarketplaceManager marketplaceManager, ILogger<BaseController> logger)
			: base(logger)
		{
			_marketplaceManager = marketplaceManager;
		}

		public IActionResult Index()
		{
			var marketplaces = _marketplaceManager.Get(Request.PathBase);
			return View(marketplaces);
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(IFormFile image, MarketplaceModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await _marketplaceManager.Create(image, model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index));
				}
				catch (UserException ex)
				{
					TempData["ErrorMessage"] = ex.Message;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex.Message);
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				}
			}
			return View();
		}

		public async Task<IActionResult> Edit(int id)
		{
			var marketPlace = await _marketplaceManager.GetById(id, Request.PathBase);
			return View(marketPlace);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(MarketplaceModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await _marketplaceManager.Update(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index));
				}
				catch (UserException ex)
				{
					TempData["ErrorMessage"] = ex.Message;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex.Message);
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				}
			}
			return RedirectToAction(nameof(Edit), new RouteValueDictionary(new { id = model.MarketplaceId }));
		}
	}
}
