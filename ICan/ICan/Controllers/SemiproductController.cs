using AutoMapper;
using ICan.Business.Managers;
using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,Operator,Assembler")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class SemiproductController : BaseController
	{
		private readonly SemiproductWarehouseManager _semiproductWarehouseManager;
		private readonly SemiproductManager _semiproductManager;
		private readonly ProductManager _productManager;
		private readonly CommonManager<OptPaper> _paperManager;
		private readonly CommonManager<OptBlockType> _blockTypeManager;

		private readonly CalcManager _calcManager;

		public SemiproductController(IMapper mapper,
			UserManager<ApplicationUser> userManager,
			CalcManager calcManager,
			SemiproductManager semiproductManager,
			CommonManager<OptPaper> paperManager,
			CommonManager<OptBlockType> blockTypeManager,
			ProductManager productManager,
			ILogger<BaseController> logger, SemiproductWarehouseManager semiproductWarehouseManager) : base(mapper, userManager, logger)
		{
			_semiproductWarehouseManager = semiproductWarehouseManager;
			_calcManager = calcManager;
			_semiproductManager = semiproductManager;
			_paperManager = paperManager;
			_blockTypeManager = blockTypeManager;
			_productManager = productManager;
		}

		public async Task<IActionResult> Index()
		{
			var list = await _semiproductManager.GetSemiproductList();
			return View(list);
		}

		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			ViewData["ActionType"] = ActionType.Details;
			var model = await _semiproductManager.GetSemiproductAsync(id.Value);
			return View(model);
		}

		public async Task<IActionResult> Create()
		{
			var model = new SemiproductModel();
			await SetViewData();
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Name,ProductId,PaperId,SemiproductTypeId,FormatId,StripNumber," +
			"Description,HaveWDVarnish,HaveStochastics,SemiproductPapers,CutLength,IsUniversal,RelatedProducts,BlockTypeId")] SemiproductModel model)
		{
			try
			{
				var isUnique = _semiproductManager.CheckModel(model, out var error);
				if (ModelState.IsValid && isUnique)
				{
					await _semiproductManager.Create(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index));
				}
				else
				{
					TempData["ErrorMessage"] = isUnique ? GetErrors() : error;
				}
			}
			catch (UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"[Semiproduct] {Const.ErrorMessages.CantSaveForUser}");
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
			}
			await SetSemiproductModelValues(model);
			await SetViewData();
			return View(model);
		}

		public async Task<IActionResult> Edit(int id)
		{
			var model = await _semiproductManager.GetSemiproductAsync(id);
			await SetViewData();
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(long id,
			[Bind("SemiproductId,Name,ProductId,PaperId,SemiproductTypeId,FormatId,StripNumber,Description,HaveWDVarnish," +
			"HaveStochastics,SemiproductPapers,CutLength,IsUniversal,RelatedProducts,BlockTypeId")] SemiproductModel model)
		{
			if (id != model.SemiproductId)
			{
				return NotFound();
			}

			var isUnique = _semiproductManager.CheckModel(model, out var error);

			if (ModelState.IsValid && isUnique)
			{
				try
				{
					await _semiproductManager.Update(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				}
				catch (UserException ex)
				{
					TempData["ErrorMessage"] = ex.Message;
					model.SemiproductPapers = await _semiproductManager.GetSemiproductPapers(model.SemiproductPapers.Select(x => x.SemiproductPaperId));
					await SetViewData();
					return View(model);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, Const.ErrorMessages.CantSaveForUser);
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				}
				return RedirectToAction(nameof(Index));
			}
			else
			{
				TempData["ErrorMessage"] = isUnique ? GetErrors() : error;
			}
			await SetSemiproductModelValues(model);
			await SetViewData();
			return View(model);
		}


		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _semiproductManager.DeleteAsync(id);
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"[Semiproduct] {Const.ErrorMessages.CantDeleteForUser}");
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteForUser;
			}
			return RedirectToAction(nameof(Index));
		}

		public IActionResult GetAvailablePapers([FromQuery(Name = "existingIds[]")] IEnumerable<long> existingIds)
		{
			var papers =
				_semiproductManager.GetAvailablePapers(existingIds);

			var list = papers.Select(paper =>
				new SelectListItem { Text = paper.Name, Value = paper.PaperId.ToString() });
			return Ok(list);
		}

		public IActionResult GetAvailableProducts([FromQuery(Name = "existingIds[]")] IEnumerable<long> existingIds, int current)
		{
			var products = _productManager.GetNotebooks().Where(notebook => notebook.ProductId != current);
			var productsForList = products.Where(x => !existingIds.Contains(x.ProductId));
			var list = productsForList.Select(product =>
				new SelectListItem { Text = product.DisplayName, Value = product.ProductId.ToString() });
			return Ok(list);
		}

		public IActionResult GetSecondPartOfBox(int semiproductId)
		{
			var otherBoxPart = _semiproductManager.GetSecondPartOfBox(semiproductId);
			var list = new List<SemiproductModel> { otherBoxPart };
			return Ok(list.Select(x => new { text = x.DisplayName, value = x.SemiproductId, typeId = x.SemiproductTypeId }));
		}

		private async Task SetSemiproductModelValues(SemiproductModel model)
		{
			if (model.SemiproductPapers != null)
			{
				var paperList = await _paperManager.GetAsync();
				foreach (var sPaper in model.SemiproductPapers)
				{
					sPaper.Paper = paperList.First(pp => pp.PaperId == sPaper.PaperId);
				}
			}
			if (model.RelatedProducts != null)
			{
				var products = _productManager.GetNotebooks();
				foreach (var rProduct in model.RelatedProducts)
				{
					rProduct.Product = products.First(prod => rProduct.ProductId == prod.ProductId);
				}
			}
		}

		private async Task SetViewData()
		{
			var notebooks = _productManager.GetNotebooks();
			ViewData["ProductId"] =
				 _mapper.Map<IEnumerable<SelectProductModel>>(notebooks);

			ViewData["PaperId"] = _semiproductManager.GetPaper()
				.Select(paper => new SelectListItem { Text = paper.Name, Value = paper.PaperId.ToString() });

			ViewData["FormatId"] = _semiproductManager.GetFormat()
				.Select(format => new SelectListItem { Text = format.Name, Value = format.FormatId.ToString() });

			ViewBag.BlockTypeId= (await _blockTypeManager.GetAsync<BlockTypeModel>())
				.Select(format => new SelectListItem { Text = format.Name, Value = format.BlockTypeId.ToString() });

			ViewData["SemiproductTypeId"] =
				_semiproductManager.GetSemiproductType(false)
				.Select(stype => new SelectListItem { Text = stype.Name, Value = stype.SemiproductTypeId.ToString() });
		}
	}
}
