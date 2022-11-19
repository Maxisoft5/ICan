using ICan.Business.Managers;
using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class CountryController : BaseController
	{
		private readonly CommonManager<OptCountry> _manager;
		public CountryController( 
			CommonManager<OptCountry> manager,
			ILogger<BaseController> logger) : base(logger)
		{
			_manager = manager;
		}

		public async Task<IActionResult> Index()
		{
			var list = await _manager.GetAsync<CountryModel>();
			return View(list);
		}

		public IActionResult Create()
		{
			var model = new CountryModel();
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("CountryId,Name,Prefix")]
				CountryModel model)
		{
			if (ModelState.IsValid)
			{
				await _manager.AddAsync(model);
				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				return RedirectToAction(nameof(Index));
			}
			return View(model);
		}	

		public async Task<IActionResult> Edit(int id)
		{
			var model = await _manager.GetAsync<int, CountryModel>(id);
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit([Bind("CountryId,Name,Prefix")]
				CountryModel model)
		{
			if (ModelState.IsValid)
			{
				await _manager.UpdateAsync(model);
				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
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
				await _manager.DeleteAsync(id);
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
