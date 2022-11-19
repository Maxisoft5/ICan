using AutoMapper;
using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ICan.Common.Models.Opt;
using ICan.Common;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class DiscountController : BaseController
	{
		private readonly DiscountManager _discountManager;
		public DiscountController(IMapper mapper,
			UserManager<ApplicationUser> userManager,
			ILogger<BaseController> logger,
			DiscountManager discountManager) : base(mapper, userManager, logger)
		{
			_discountManager = discountManager;
		}

		public async Task<IActionResult> Index()
		{
			var discounts = await _discountManager.GetDiscounts();
			return View(discounts);
		}

		public async Task<IActionResult> Details(int id)
		{
			var discount = await _discountManager.GetDiscount(id);

			if (discount == null)
			{
				return NotFound();
			}

			ViewData["ActionType"] = ActionType.Details;

			return View("Details", discount);
		}

		public IActionResult Create()
		{
			ViewBag.Action = ActionType.Creation;
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Value,Enabled,Description")]
				DiscountModel model)
		{
			if (ModelState.IsValid)
			{
				await _discountManager.Create(model);
				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				return RedirectToAction(nameof(Index));
			}
			TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
			return View(model);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _discountManager.Delete(id);
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			}
			catch (UserException ex)
			{
				ViewData["ErrorMessage"] = ex.Message;
			}
			return RedirectToAction(nameof(Index));
		}
	}
}
