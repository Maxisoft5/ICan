using AutoMapper;
using ICan.Business.Managers;
using ICan.Common;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Common.Utils;
using ICan.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,Operator,Assembler,Designer")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class PrintOrderController : BaseController
	{
		private static readonly List<int> CoversAndCursors = new List<int> { (int)SemiProductType.Covers, (int)SemiProductType.Cursor };

		private readonly SemiproductWarehouseManager _semiproductWarehouseManager;
		private readonly SemiproductManager _semiproductManager;
		private readonly PaperOrderManager _paperOrderManager;

		private readonly PrintOrderManager _printOrderManager;
		private readonly CalcManager _calcManager;

		public PrintOrderController(IMapper mapper, UserManager<ApplicationUser> userManager,
			SemiproductWarehouseManager semiproductWarehouseManager,
			CalcManager calcManager,
			PaperOrderManager paperOrderManager,
			SemiproductManager semiproductManager,
			PrintOrderManager printOrderManager,
			ILogger<BaseController> logger) : base(mapper, userManager, logger)
		{
			_semiproductWarehouseManager = semiproductWarehouseManager;
			_printOrderManager = printOrderManager;
			_calcManager = calcManager;
			_semiproductManager = semiproductManager;
			_paperOrderManager = paperOrderManager;
		}

		public IActionResult Index()
		{
			IEnumerable<PrintOrderModel> list = _printOrderManager.Get();
			return View(list);
		}

		[Authorize(Roles = "Admin,Operator,Assembler")]
		public async Task<IActionResult> Create()
		{
			if (User.IsInRole(Const.Roles.Designer))
				return Forbid();
			ViewBag.Action = ActionType.Creation;
			var model = new PrintOrderModel
			{
				OrderDate = DateTime.Now,
				CanEditSemiproducts = true,
				CanDeleteIncomes = true
			};
			await SetViewData();

			return View("Edit", model);
		}

		[Authorize(Roles = "Admin,Operator,Assembler")]
		public async Task<IActionResult> AddIncoming(int id)
		{
			var model = new PrintOrderIncomingModel
			{
				IncomingDate = DateTime.Now,
				PrintOrderId = id,
				PrintOrderIncomingItems = await _printOrderManager.GetEmptyItems(id),
			};
			await SetIncomingViewData(id);
			return View(model);
		}

		[HttpPost]
		[Authorize(Roles = "Admin,Operator,Assembler")]
		public async Task<IActionResult> AddIncoming(PrintOrderIncomingModel model)
		{
			if (CheckOrderIncomingModel(model, out var errorMessage))
			{
				try
				{
					await _printOrderManager.AddIncoming(model);
					TempData["StatusMessage"] = "Информация успешно сохранена";
				}
				catch
				{
					TempData["ErrorMessage"] = "Произошла ошибка во время сохранения информации";
				}
			}
			else
			{
				TempData["ErrorMessage"] = errorMessage;
			}

			return RedirectToAction(nameof(Edit), new { id = model.PrintOrderId });
		}

		[AjaxOnly]
		[HttpPost]
		[Authorize(Roles = "Admin,Operator,Assembler")]
		public async Task<IActionResult> DeleteIncoming(int id)
		{
			try
			{
				await _printOrderManager.DeleteIncoming(id);
				return Ok();
			}
			catch (UserException ex)
			{
				var errorString = ex.Message;
				_logger.LogWarning(ex, errorString);
				return BadRequest(errorString);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "При удалении прихода возникла ошибка");
				return BadRequest();
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin,Operator,Assembler")]
		public async Task<IActionResult> Create(PrintOrderModel model)
		{
			await SetViewData();
			ViewBag.Action = ActionType.Creation;

			var ok = CheckModel(model, out var errorList);
			if (ModelState.IsValid && ok)
			{
				try
				{
					await _printOrderManager.CreateAsync(model);
					TempData["StatusMessage"] = "Заказ печати успешно добавлен";
					return RedirectToAction(nameof(Index));
				}
				catch (UserException ex)
				{
					errorList = new List<string> { { ex.Message } };
				}
				catch (Exception ex)
				{
					var systemErrorList = new List<string>
					{
						Const.ErrorMessages.CantSaveForUser
					};
					errorList = systemErrorList;
					_logger.LogError(ex, string.Format(Const.ErrorMessages.CantSave,
						"Create", this.GetType().ToString(), model.PrintOrderId));
				}
			}

			await SetSemiproductData(model);
			await SetPaperOrders(model);
			model.CanEditSemiproducts = true;
			model.CanDeleteIncomes = true;
			TempData["ErrorMessage"] = string.Join(", ", errorList);
			return View("Edit", model);
		}

		public async Task<IActionResult> AvailableSemiproducts(string existingIds, int semiproductTypeId)
		{
			var existingList = existingIds?.Split(",").Select(item => int.Parse(item)) ?? Enumerable.Empty<int>();
			if (semiproductTypeId == 0 && existingList.Any())
			{
				semiproductTypeId = (await _semiproductManager.GetSemiproductAsync(existingList.First())).SemiproductTypeId;
			}
			IEnumerable<int> compatibleSemiProductTypes = GetCompatibleSemiProductTypes(semiproductTypeId);
			var semiProducts = _semiproductManager.GetSemiproductsRaw().Where(sProduct => compatibleSemiProductTypes.Contains(sProduct.SemiproductTypeId));

			if (existingList != null && existingList.Any())
			{

				semiProducts = semiProducts
					.Where(sProduct => !existingList.Contains(sProduct.SemiproductId));
			}

			var modelList = _mapper.Map<IEnumerable<SemiproductModel>>(semiProducts);
			var list = modelList.Select(sProduct =>
				new { Text = sProduct.DisplayName, Value = sProduct.SemiproductId.ToString(), TypeId = sProduct.SemiproductTypeId });
			return Ok(list);
		}


		public async Task<IActionResult> AvailablePaperOrders(string existingIds, int semiProductId)
		{
			var existingList = existingIds?.Split(",").Select(item => long.Parse(item)) ?? Enumerable.Empty<long>();

			IEnumerable<PaperOrderModel> paperOrders = await _paperOrderManager.GetPaperOrdersForPrintOrderAsync(existingList, semiProductId);

			var list = paperOrders.Where(paperOrder => paperOrder.SheetsLeftAmount > 0)
				.Select(paperOrder =>
				new SelectListItem { Text = paperOrder.ToString(), Value = paperOrder.PaperOrderId.ToString() });

			return Ok(list);
		}

		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}
			var model = await _printOrderManager.GetAsync(id.Value);
			var raw = await _printOrderManager.GetRawAsync(id.Value);
			if (model == null)
			{
				return NotFound();
			}
			ViewBag.Action = ActionType.Edition;
			ViewBag.PrintOrderId = id.Value;


			if (model.PrintOrderPapers != null && model.PrintOrderPapers.Any())
			{
				model.PrintOrderPapers.ForEach(printOrderPaper =>
				{
					var thisPrintOrderP = raw.PrintOrderPapers.First(p => p.PrintOrderPaperId == printOrderPaper.PrintOrderPaperId);

					printOrderPaper.PaperOrder = $"Заказ {thisPrintOrderP.PaperOrder.Paper.Name} от {thisPrintOrderP.PaperOrder.OrderDate.ToShortDateString()}, осталось  { thisPrintOrderP.PaperOrder.SheetCount - thisPrintOrderP.PaperOrder.PrintOrderPapers.Sum(q => q.SheetsTakenAmount)}";
				});
				model.PaperName = raw.PrintOrderPapers.FirstOrDefault()?.PaperOrder.Paper.Name;
			}

			if (model.PrintOrderSemiproducts != null && model.PrintOrderSemiproducts.Any())
			{
				_printOrderManager.SetSemiproducts(model);
				_printOrderManager.SetPrintOrderIncomings(model);
			}
			var used = _printOrderManager.UsedInNotchOrders(id.Value);
			model.CanEditSemiproducts = (model.PrintOrderIncomings == null || !model.PrintOrderIncomings.Any())
				&& !used;

			model.CanDeleteIncomes = !used;

			await SetViewData();
			return View("Edit", model);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, PrintOrderModel model)
		{
			if (id != model.PrintOrderId)
			{
				return NotFound();
			}
			var errorList = new List<string>();
			var ok = CheckModel(model, out errorList);
			if (ModelState.IsValid && ok)
			{
				try
				{
					await _printOrderManager.EditAsync(model, User);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index));
				}
				catch (UserException ex)
				{
					var errorString = ex.Message;
					errorList = new List<string> { errorString };
					_logger.LogWarning(ex, errorString);
				}
				catch (Exception ex)
				{
					var systemErrorList = new List<string>
					{
						Const.ErrorMessages.CantSaveForUser,
					};
					errorList = systemErrorList;
					_logger.LogError(ex, string.Format(
						Const.ErrorMessages.CantSave,
						"Edit",
						this.GetType().ToString(),
						model.PrintOrderId));
				}
			}
			await SetViewData();
			await SetPaperOrders(model);
			errorList.Add(base.GetErrors());
			TempData["ErrorMessage"] = string.Join(", ", errorList);
			ViewBag.Action = ActionType.Edition;

			_printOrderManager.SetSemiproducts(model);
			model.PrintOrderIncomings = (await _printOrderManager.GetPrintOrderIncomingsAsync(id)).ToList();
			model.PrintOrderPayments = (await _printOrderManager.GetPrintOrderPaymentsAsync(id)).ToList();
			var used = _printOrderManager.UsedInNotchOrders(model.PrintOrderId);
			model.CanEditSemiproducts = (model.PrintOrderIncomings == null || !model.PrintOrderIncomings.Any())
				&& !used;
			model.CanDeleteIncomes = !used;
			ViewData["PrintOrderId"] = model.PrintOrderId;
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin,Operator,Assembler")]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _printOrderManager.DeleteAsync(id);
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			}
			catch (UserException ex)
			{
				var errorString = ex.Message;
				TempData["ErrorMessage"] = errorString;
				_logger.LogError(ex, errorString);
			}
			catch (Exception ex)
			{
				var errorString = "При удалении возникла ошибка";
				TempData["ErrorMessage"] = errorString;
				_logger.LogError(ex, errorString);
			}
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[Authorize(Roles = "Admin,Operator,Assembler")]
		public async Task<IActionResult> AddPayment(decimal amount, string date, int printOrderId)
		{
			if (amount > 0 && DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None,
				out var paymentDate))
			{
				var model = new PrintOrderPaymentModel { Amount = amount, Date = paymentDate, PrintOrderId = printOrderId };
				model.PrintOrderPaymentId = await _printOrderManager.AddPayment(model);
				return Ok(model);
			}

			return BadRequest();
		}

		[HttpDelete]
		[Authorize(Roles = "Admin,Operator,Assembler")]
		public async Task<IActionResult> DeletePayment(int printOrderPaymentId)
		{
			await _printOrderManager.DeletePayment(printOrderPaymentId);
			return Ok();
		}

		public IActionResult GetPaperName([FromQuery(Name = "semiproductIds[]")] List<int> semiproductIds)
		{
			if (semiproductIds == null || !semiproductIds.Any())
				return NoContent();

			var semiProducts = _semiproductManager.GetSemiproducts(semiproductIds);
			if (semiProducts == null || !semiProducts.Any())
				return NoContent();

			var paperNames = string.Join(",", semiProducts.SelectMany(semiproduct => semiproduct.SemiproductPapers).Select(sPaper => sPaper.Paper.Name).Distinct());
			var haveWDVarnish = semiProducts.First().HaveWDVarnish;
			var haveStochastics = semiProducts.First().HaveStochastics;

			var result = new { paperNames, haveWDVarnish, haveStochastics };
			return Ok(result);
		}

		[Authorize(Roles = "Admin,Operator,Assembler")]
		public async Task<IActionResult> GetArrearsReport()
		{
			var report = await _printOrderManager.GetArrearsReport();
			return File(report, MediaTypeNames.Application.Octet, $"Отчёт по задолженности от {DateTime.Now.ToShortDateString()}.xlsx");
		}

		public async Task<IActionResult> Report(int id)
		{
			var model = await _printOrderManager.GetReportAsync(id);
			return View(model);
		}

		private async Task SetSemiproductData(PrintOrderModel model)
		{
			if (model.PrintOrderSemiproducts != null)
			{
				var list = await _semiproductManager.GetSemiproductList();
				foreach (var item in model.PrintOrderSemiproducts
					.Where(sProduct => sProduct.PrintOrderSemiproductId == 0))
				{
					item.SemiProduct = list
						.First(sProd => sProd.SemiproductId == item.SemiproductId);
				}
			}
		}


		private bool CheckModel(PrintOrderModel model, out List<string> errorList)
		{
			errorList = new List<string>();
			if (model.PrintOrderPapers == null || !model.PrintOrderPapers.Any())
			{
				errorList.Add("Невозможно сохранить заказ без заказа бумаги");
			}
			else if (model.PrintOrderPapers.Any(paperOrder => paperOrder.SheetsTakenAmount <= 0))
			{
				errorList.Add("В заказе бумаги необходимо указать количество бумаги");
			}
			if (model.PrintOrderSemiproducts != null && model.PrintOrderSemiproducts.Any())
			{
				var semiProductsInOrder = model.PrintOrderSemiproducts.Select(sProd => sProd.SemiproductId).ToList();
				var selectedSemiproducts = _semiproductManager.GetSemiproducts(semiProductsInOrder);

				var semiproductTypes = selectedSemiproducts.Select(sProd => sProd.SemiproductTypeId).Distinct();
				var comppatible = GetCompatibleSemiProductTypes(semiproductTypes.First());
				if (semiproductTypes.Count() > 1 && (comppatible.Except(semiproductTypes).Any() || semiproductTypes.Except(comppatible).Any()))
				{
					errorList.Add("В заказе не могут присутствовать полуфабрикаты разного типа");
				}
				else if (semiproductTypes.First() == (int)SemiProductType.Block &&
					selectedSemiproducts.Count() > 1)
				{
					errorList.Add("В заказе может быть ровно один блок");
				}
				else if (selectedSemiproducts.Any(x => x.SemiproductTypeId == (int)SemiProductType.Box))
				{
					var productSeries = selectedSemiproducts
						.Where(semiproduct => semiproduct.SemiproductTypeId == (int)SemiProductType.Box)
						.Select(semiproduct => semiproduct.ProductSeriesId)
						.Distinct();
					if (productSeries.Count() > 1)
					{
						errorList.Add("Полуфабрикаты типа 'Коробка' должны быть одной серии");
					}
				}
			}
			else
			{
				errorList.Add("Невозможно сохранить заказ без полуфабрикатов");
			}

			return !errorList.Any();
		}

		private async Task SetViewData()
		{
			var paperOrders = await _paperOrderManager.GetPaperOrdersForPrintOrderAsync();
			ViewData["PaperOrderId"] = paperOrders.Select(paperO => new SelectListItem { Text = $"Заказ от {paperO.OrderDate.ToShortDateString()} {paperO.PaperName} {paperO.FormatName}, кол-во {paperO.SheetCount} ", Value = paperO.PaperOrderId.ToString() });

			var semiprodTypes = _semiproductManager.GetSemiproductType();
			ViewData["SemiproductTypes"] = semiprodTypes.Select(type => new SelectListItem { Text = type.Name, Value = type.SemiproductTypeId.ToString() });
		}

		private async Task SetPaperOrders(PrintOrderModel model)
		{
			if (model.PrintOrderPapers != null && model.PrintOrderPapers.Any())
			{
				foreach (var printOrderPaper in model.PrintOrderPapers)
				{
					var paperOrder = await _paperOrderManager.GetAsync(printOrderPaper.PaperOrderId);

					if (paperOrder != null)
						printOrderPaper.PaperOrder = $"Заказ {paperOrder.PaperName} от {paperOrder.OrderDate.ToShortDateString()}, осталось  { paperOrder.SheetCount - paperOrder.PrintOrderPapers.Sum(q => q.SheetsTakenAmount)}";
				}
			}
		}

		private bool CheckOrderIncomingModel(PrintOrderIncomingModel model, out string errorMessage)
		{
			errorMessage = string.Empty;

			if (model.PrintOrderIncomingItems.Any(x => x.Amount < 0))
			{
				errorMessage = $"Кол-во полуфабрикатов не может быть отрицательным";
				return false;
			}

			var printOrder = _printOrderManager.GetAsync(model.PrintOrderId).Result;
			var printOrderIncomingItems = printOrder.PrintOrderIncomings
					.SelectMany(pOrderIncoming =>
							pOrderIncoming.PrintOrderIncomingItems);
			return true;
		}

		private static IEnumerable<int> GetCompatibleSemiProductTypes(int semiproductTypeId)
		{
			var smType = (SemiProductType)semiproductTypeId;
			switch (smType)
			{
				case SemiProductType.Covers:
					return new List<int> { (int)SemiProductType.Covers, (int)SemiProductType.Cursor }; ;

				case SemiProductType.Cursor:
					return new List<int> { (int)SemiProductType.Covers, (int)SemiProductType.Box, (int)SemiProductType.Cursor };
				case SemiProductType.Box:
					return new List<int> { (int)SemiProductType.Pointer, (int)SemiProductType.Box, (int)SemiProductType.Cursor };
				case SemiProductType.Pointer:
					return new List<int> { (int)SemiProductType.Box,
						(int)SemiProductType.Cursor,
						(int)SemiProductType.Pointer };
				default:
					return new List<int> { semiproductTypeId };
			}
		}


		private async Task SetIncomingViewData(int id)
		{
			var availableItems = new List<SelectListItem>
			{
				new SelectListItem {Text = PrintOrderIncomingType.Ordinal.GetDisplayName(),
					Value = ((int)PrintOrderIncomingType.Ordinal).ToString(), },
			};

			var existing = await _printOrderManager.GetPrintOrderIncomingsAsync(id);
			if (existing != null && existing.Any())
			{
				availableItems.Add(new SelectListItem
				{
					Text = PrintOrderIncomingType.OverPrint.GetDisplayName(),
					Value = ((int)PrintOrderIncomingType.OverPrint).ToString(),
				});
				availableItems.Add(new SelectListItem
				{
					Text = PrintOrderIncomingType.Flaw.GetDisplayName(),
					Value = ((int)PrintOrderIncomingType.Flaw).ToString(),
				});
			};

			ViewData["IncomingTypes"] = availableItems;
		}
	}
}
