using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ICan.Common.Models.Opt;
using ICan.Common;
using AutoMapper;
using ICan.Common.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,Operator,Assembler,Storekeeper")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class AssemblyController : BaseController
	{
		private readonly AssemblyManager _assemblyManager;
		private readonly CommonManager<OptProductseries> _productSeriesManager;

		public AssemblyController(
			AssemblyManager assemblyManager,
			IMapper mapper,
			UserManager<ApplicationUser> userManager,
			ILogger<BaseController> logger,
			CommonManager<OptProductseries> productSeriesManager) : base(mapper, userManager, logger)
		{
			_assemblyManager = assemblyManager;
			_productSeriesManager = productSeriesManager;
		}

		public async Task<IActionResult> Index()
		{
			var assemblies = await _assemblyManager.GetAssembliesAsync();
			return View(assemblies);
		}

		public async Task<IActionResult> Create(AssemblyType assemblyType)
		{
			ViewBag.Action = ActionType.Creation;
			var products = await _assemblyManager.GetProductsForAssembly(assemblyType);
			var selectList = products.Select(product => new SelectListItem { Text = product.DisplayName, Value = product.ProductId.ToString() });
			ViewData["ProductId"] = selectList;

			await SetProductSeriesList();
			var model = new AssemblyModel
			{
				Date = DateTime.Now,
				ProductId = int.Parse(selectList.First().Value),
				AssemblyType = assemblyType,
			};

			await SetSemiproductsAndPrintOrdersForProduct(model, true);
			string viewName = GetViewName(model.AssemblyType);
			return View(viewName, model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(AssemblyModel model)
		{
			ViewBag.Action = ActionType.Creation;
			string viewName = GetViewName(model.AssemblyType);
			if (ModelState.IsValid)
			{
				try
				{
					await CheckAssemblySemiproducts(model, model.Amount);
					await _assemblyManager.CreateAsync(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index));
				}
				catch (UserException ex)
				{
					TempData["ErrorMessage"] = ex.Message;
				}
				catch (Exception ex)
				{
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
					_logger.LogError(ex, "[Сборка] ошибка при создании");
				}
			}

			await SetSemiproductsAndPrintOrdersForProduct(model, true);
			await SetViewData(model.AssemblyType);
			await SetProductSeriesList();
			return View(viewName, model);
		}

		public async Task<IActionResult> Edit(long id)
		{
			ViewBag.Action = ActionType.Edition;

			if (id == 0)
				return NotFound();

			var model = await _assemblyManager.GetAssemblyAsync(id);
			await SetSemiproductsAndPrintOrdersForProduct(model, model.ConsiderNotch);
			await SetViewData(model.AssemblyType);
			var viewName = model.CanEdit ? "Edit" : "Details";
			return View(viewName, model);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(AssemblyModel model)
		{
			var raw = await _assemblyManager.GetAssemblyAsync(model.AssemblyId);
			var amount = raw.Amount > model.Amount ? 0 : (model.Amount - raw.Amount);

			ViewBag.Action = ActionType.Edition;
			if (ModelState.IsValid)
			{
				try
				{
					await CheckAssemblySemiproducts(model, amount);

					await _assemblyManager.EditAsync(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index));
				}
				catch (UserException ex)
				{
					ModelState.AddModelError("UserError", ex.Message);
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("InnerError", Const.ErrorMessages.CantSaveForUser);
					_logger.LogError(ex, "[Сборка] ошибка при создании");
				}
			}
			_assemblyManager.SetAssemblySemiproducts(model);
			model.Product = raw.Product;
			await SetSemiproductsAndPrintOrdersForProduct(model, model.ConsiderNotch);
			await SetViewData(model.AssemblyType);
			TempData["ErrorMessage"] = GetErrors();
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(long id)
		{
			if (id == 0)
				return NotFound();

			try
			{
				await _assemblyManager.DeleteAsync(id);
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			}
			catch (UserException ex)
			{
				_logger.LogError($"Возникла ошибка при удалении сборки с id {id}", ex);
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Возникла ошибка при удалении сборки с id {id}", ex);
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDeleteForUser;
			}

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> GetSemiproductsAndOrders(int productId, int assemblyId)
		{
			var considerNotch = _assemblyManager.ConsiderNotch(assemblyId);

			var semiproductsWithPrintOrders = await _assemblyManager.GetSemiproductsAndOrdersAsync(productId, considerNotch);

			var result = semiproductsWithPrintOrders
				.Select(x => new
				{
					Semiproduct = _mapper.Map<SemiproductShortModel>(x.Key),
					Orders = x.Value.Select(x => new KeyValuePair<int, string>(x.Key, x.Value))
				}).ToList();
			return Ok(result);
		}

		public async Task<IActionResult> GetProductsByProductSeries(int productSeriesId)
		{
			var products = await _assemblyManager.GetProductsForAssembly(AssemblyType.Assembly);
			if (productSeriesId != 0)
				products = products.Where(x => x.ProductSeriesId == productSeriesId);
			return Ok(products.Select(x => new { Text = x.DisplayName, Value = x.ProductId }));
		}

		private async Task SetSemiproductsAndPrintOrdersForProduct(AssemblyModel model, bool considerNotch)
		{
			var semiproductsWithPrintOrders = await _assemblyManager.GetSemiproductsAndOrdersAsync(model.ProductId, considerNotch);
			model.SemiproductsWithPrintOrders = semiproductsWithPrintOrders;
		}

		private async Task SetViewData(AssemblyType assemblyType)
		{
			var products = await _assemblyManager.GetProductsForAssembly(assemblyType);
			ViewData["ProductId"] = products.Select(product => new SelectListItem { Text = product.DisplayName, Value = product.ProductId.ToString() });
		}

		private async Task SetProductSeriesList()
		{
			var series = await _productSeriesManager.GetAsync<ProductSeriesModel>();
			var seriesList = series.Where(x => x.ProductKindId == (int)ProductKind.Notebook
								&& !Const.AssemblesAsKitSeriesIds.Contains(x.ProductSeriesId))
						.OrderBy(productSeries => productSeries.Order)
						.Select(series => new SelectListItem { Text = series.Name, Value = series.ProductSeriesId.ToString() });
			ViewData["ProductSeries"] = seriesList;
		}

		private async Task CheckAssemblySemiproducts(AssemblyModel model, int requiredamount)
		{
			var printOrderAssemblyInfo = await _assemblyManager.GetAssemblyInfo(model, requiredamount);
			var  errors = new StringBuilder();
		
			bool enoughSprings = _assemblyManager.CheckSprings(model.Amount);

			if (!printOrderAssemblyInfo.CanAssembly || !enoughSprings)
			{
			 errors.Append(GetEnoughSemiproductsStringError(printOrderAssemblyInfo));
			}

			if (!enoughSprings)
			{
				errors.Append("недостаточно пружин на складе");

			}
			if (errors.Length != 0)
				throw new UserException(errors.ToString());

		}

		private string GetEnoughSemiproductsStringError(PrintOrderAssemblyInfo printOrderAssemblyInfo)
		{
			var sProducts = new List<string>();
			var errorStrings = new List<string>();
			//склад
			if (!printOrderAssemblyInfo.IsEnoughBlocksAtWarehouse)
			{
				sProducts.Add("блоков");
			}
			if (printOrderAssemblyInfo.NeedCheckStickers && !printOrderAssemblyInfo.IsEnoughStickersAtWarehouse)
			{
				sProducts.Add("наклеек");
			}
			if (printOrderAssemblyInfo.NeedChekCovers && !printOrderAssemblyInfo.IsEnoughCoversAtWarehouse)
			{
				sProducts.Add("обложек");
			}

			if (sProducts.Any())
				errorStrings.Add($"недостаточно полуфабрикатов на складе: { string.Join(", ", sProducts)}");

			//заказы печати
			sProducts = new List<string>();
			if (!printOrderAssemblyInfo.IsEnoughBlocksInPrintOrderIncoming)
			{
				sProducts.Add("блоков");
			}
			if (printOrderAssemblyInfo.NeedCheckStickers && !printOrderAssemblyInfo.ConsiderNotch && !printOrderAssemblyInfo.IsEnoughStickersInPrintOrderIncoming)
			{
				sProducts.Add("наклеек");
			}
			if (printOrderAssemblyInfo.NeedChekCovers && !printOrderAssemblyInfo.IsEnoughCoversInPrintOrderIncoming)
			{
				sProducts.Add("обложек");
			}

			if (sProducts.Any())
				errorStrings.Add($"недостаточно полуфабрикатов в приходах заказа печати/надсечки: { string.Join(", ", sProducts)}");

			if (printOrderAssemblyInfo.ConsiderNotch && !printOrderAssemblyInfo.IsEnoughStickersInNotchOrderIncoming)
			{
				errorStrings.Add($"недостаточно наклеек в приходе заказе надсечки");
			}
			return $"Невозможно сохранить информацию: {string.Join(", ", errorStrings)}";
		}

		private static string GetViewName(AssemblyType assemblyType)
		{
			return assemblyType == AssemblyType.Assembly
				? "CreateAssembly"
				: "CreateWind";
		}

	}
}
