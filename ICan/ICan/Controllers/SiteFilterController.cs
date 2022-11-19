using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Domain;
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
	[Authorize(Roles = "Admin,ContentMan")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class SiteFilterController : BaseController
	{
		private readonly SiteFilterManager _siteFilterManager;

		public SiteFilterController(SiteFilterManager siteFilterManager,
			UserManager<ApplicationUser> userManager,
			ILogger<BaseController> logger) : base(userManager, logger)
		{
			_siteFilterManager = siteFilterManager;
		}

		public async Task<IActionResult> Index()
		{
			var list = await _siteFilterManager.GetAllAsync();
			return View(list);
		}

		public async Task<IActionResult> Create(SiteFilterModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await _siteFilterManager.AddAsync(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index)); 
				}
				catch (Exception ex) 
				{
					TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteForUser;
					_logger.LogError(ex, $"[SiteFilter] error in adding filter {model.Name}");
				}
			}
			return View(model);
		}

		public async Task<IActionResult> Edit(int id)
		{
			var model = await _siteFilterManager.GetByIdAsync(id);
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			var filter = await _siteFilterManager.GetByIdAsync(id);
			if (filter.IsInternal)
			{
				TempData["ErrorMessage"] = "Фильтр, использующийся на главной странице сайта, невозможно удалить";

				return RedirectToAction(nameof(Index));
			}

			try
			{
				await _siteFilterManager.DeleteAsync(id);
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteForUser;
				_logger.LogError(ex, $"[SiteFilterProduct] error in deleting filter{id}");
			}
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> DeleteProduct(int siteFilterId, int siteFilterProductId)
		{
			try
			{
				await _siteFilterManager.DeleteProductAsync(siteFilterProductId);
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				_logger.LogError(ex, $"[SiteFilterProduct] error in deleting {nameof(siteFilterId)} {siteFilterId} {nameof(siteFilterProductId)} {siteFilterProductId}");
			}
			return RedirectToAction(nameof(Edit), new { id = siteFilterId });
		}

		public async Task<IActionResult> AddFilterProduct(int id)
		{
			var model = new SiteFilterProductModel
			{
				SiteFilterId = id,
				AvailableProducts = await _siteFilterManager.GetAvailableProductsAsync(id)
			};
			return PartialView("_AddFilterProduct", model);
		}

		[HttpPost]
		public async Task<IActionResult> AddFilterProduct(SiteFilterProductModel model)
		{
			try
			{
				await _siteFilterManager.AddFilterProductAsync(model);
				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				_logger.LogError(ex, $"[SiteFilterProduct] error in adding site filter product {nameof(model.SiteFilterId)} {model.SiteFilterId} {nameof(model.ProductId)} {model.ProductId}");
			}
			return RedirectToAction(nameof(Edit), new { id = model.SiteFilterId });

		}	
		
		public async Task<IActionResult> EditFilterProduct(int siteFilterProductId)
		{
			SiteFilterProductModel model = await _siteFilterManager.GetFilterProduct(siteFilterProductId);
			return PartialView("_EditFilterProduct", model);
		}

		[HttpPost]
		public async Task<IActionResult> EditFilterProduct(SiteFilterProductModel model)
		{
			try
			{
				await _siteFilterManager.EditFilterProductAsync(model);
				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				return RedirectToAction(nameof(Edit), new { id = model.SiteFilterId });
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				_logger.LogError(ex, $"[SiteFilterProduct] error in editing site filter product {nameof(model.SiteFilterProductId)} {model.SiteFilterProductId} {nameof(model.ProductId)} {model.ProductId}");
			}
			return RedirectToAction(nameof(Edit), new { id = model.SiteFilterId });
		}
	}
}
