using AutoMapper;
using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Threading.Tasks;
using ICan.Common.Models.Opt;
using ICan.Common;

namespace ICan.Controllers
{
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class ProductseriesController : BaseController
	{
		private readonly CommonManager<OptProductseries> _commonManager;
		private readonly ProductManager _productManager;

		protected static JsonSerializerSettings _formattingSettings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver()
		};

		public ProductseriesController(IMapper mapper,
			UserManager<ApplicationUser> userManager,
			ILogger<BaseController> logger, CommonManager<OptProductseries> commonManager, ProductManager productManager) : base(mapper, userManager, logger)
		{
			_commonManager = commonManager;
			_productManager = productManager;
		}

		[Authorize(Roles = "Admin,ContentMan")]
		public async Task<IActionResult> Index()
		{
			var list = await _commonManager.GetAsync<ProductSeriesModel>();
			return View(list.Where(x => x.ProductKindId == Const.NoteBookProductKind));
		}

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Create()
		{
			await SetProductKind();
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Create([Bind("ProductSeriesId,SiteName,Name")] ProductSeriesModel model)
		{
			if (ModelState.IsValid)
			{
				await _commonManager.AddAsync(model);
				return RedirectToAction(nameof(Index));
			}

			return View(model);
		}

		[Authorize(Roles = "Admin,ContentMan")]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}
			await SetProductKind();

			var model = await _commonManager.GetAsync<int, ProductSeriesModel>(id.Value);
			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin,ContentMan")]
		public async Task<IActionResult> Edit(int id, [Bind("ProductSeriesId,Name,SiteName,Order")] ProductSeriesModel model)
		{
			if (id != model.ProductSeriesId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					await _commonManager.UpdateAsync(model);
				}
				catch (DbUpdateConcurrencyException ex)
				{
					if (!await OptProductseriesExists(model.ProductSeriesId))
					{
						return NotFound();
					}
					else
					{
						_logger.LogError(ex, "Error while editing product series");
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			return View(model);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Delete(int id)
		{			
			try
			{
				await _commonManager.DeleteAsync(id);
			}
			catch (DbUpdateException ex)
			{
				_logger.LogError(ex, ex.GetType().ToString());
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteForUser;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.GetType().ToString());
			}
			return RedirectToAction(nameof(Index));
		}

		private async Task<bool> OptProductseriesExists(int id)
		{
			return await _commonManager.GetAsync(id) != null;
		}

		private async Task SetProductKind()
		{
			var optProductKinds = await _productManager.GetProductKinds();
			ViewData["ProductKindId"] = new SelectList(optProductKinds.Select(t => new { Id = t.ProductKindId, t.Name }), "Id", "Name");
		}
	}
}
