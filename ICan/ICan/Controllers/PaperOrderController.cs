using ICan.Business.Managers;
using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,Assembler")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class PaperOrderController : BaseController
	{
		private readonly PaperOrderManager _paperOrderManager;
		private readonly PaperWarehouseManager _paperWarehouseManager;
		private readonly CommonManager<OptPaper> _paperManager;
		private readonly FormatManager _formatManager;

		public PaperOrderController(PaperOrderManager paperOrderManager,
			 CommonManager<OptPaper> paperManager, FormatManager formatManager,
			ILogger<BaseController> logger, PaperWarehouseManager paperWarehouseManager) : base(logger)
		{
			_paperOrderManager = paperOrderManager;
			_paperManager = paperManager;
			_formatManager = formatManager;
			_paperWarehouseManager = paperWarehouseManager;
		}

		public IActionResult Index()
		{
			IEnumerable<PaperOrderModel> modelList = _paperOrderManager.Get();
			return View(modelList);
		}

		public async Task<IActionResult> Create()
		{
			ViewBag.Action = ActionType.Creation;
			var model = new PaperOrderModel { OrderDate = DateTime.Now };
			await SetViewData();
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("OrderDate,PaperId,FormatId,SheetCount,InvoiceDate,InvoiceNum," +
			"IsPaid,SupplierCounterPartyId,RecieverCounterPartyId,OrderSum,PaidSum,Comment,Weight")] PaperOrderModel model)
		{
			if (ModelState.IsValid)
			{
				await _paperOrderManager.Create(model);
				TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				return RedirectToAction(nameof(Index));
			}
			await SetViewData();
			return View(model);
		}

		public async Task<IActionResult> Edit(int id)
		{
			if (id == 0)
			{
				return NotFound();
			}
			var model = await _paperOrderManager.GetAsync(id);
			await SetViewData();
			return View("Edit", model);
		}

		// POST: Events/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(long id,
			[Bind("PaperOrderId,OrderDate,PaperId,FormatId,SheetCount,InvoiceDate,InvoiceNum,IsPaid,SupplierCounterPartyId," +
			"RecieverCounterPartyId,OrderSum,PaidSum,Comment,Weight")]
			PaperOrderModel model)
		{
			if (id != model.PaperOrderId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					await _paperOrderManager.UpdateAsync(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
					return RedirectToAction(nameof(Index));
				}
				catch (Exception ex)
				{
					var errorString = "При сохранении возникла ошибка";
					TempData["ErrorMessage"] = errorString;
					_logger.LogError(ex, string.Format(Const.ErrorMessages.CantSave,
						"Edit", GetType().ToString(), model.PaperOrderId));
				}
			}
			await SetViewData();
			return View(model);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _paperOrderManager.DeleteAsync(id);
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			}
			catch (UserException ex) 
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = "При удалении возникла ошибка";
				_logger.LogError(ex, ex.Message);
			}
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> AddIncoming(int id)
		{
			var model = new PaperOrderIncomingModel
			{
				IncomingDate = DateTime.Now,
				PaperOrderId = id,
				WarehouseTypes = _paperWarehouseManager.GetWarehouseTypesList(),
				Amount = await _paperOrderManager.GetAvailableAmount(id)
			};
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> AddIncoming(PaperOrderIncomingModel incoming)
		{
			try
			{					
				await _paperOrderManager.AddIncoming(incoming);
				TempData["StatusMessage"] = "Информация успешно сохранена";
			}
			catch(UserException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}
			catch(Exception ex)
			{
				TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				_logger.LogInformation(ex.Message);
			}
			return RedirectToAction(nameof(Edit), new { id = incoming.PaperOrderId });
		}

		[HttpPost]
		public async Task DeleteIncoming(int id)
		{
			if (id != 0)
				await _paperOrderManager.DeleteIncoming(id);
		}

		public IActionResult GetPaperOrderWeight(int sheetCount, int paperId)
		{
			var papers = _paperManager.Get<PaperModel>("TypeOfPaper");
			var paper = papers.First(x => x.PaperId == paperId);
			var weight = PaperOrderModel.GetWeight(sheetCount, paper.Length, paper.Width, paper.TypeOfPaper.Density);
			return Ok(weight);
		}

		public IActionResult GetPaperOrderSheetCount(double weight, int paperId)
		{
			var papers = _paperManager.Get<PaperModel>("TypeOfPaper");
			var paper = papers.First(x => x.PaperId == paperId);
			var sheetCount = PaperOrderModel.GetSheetCount(weight, paper.Length, paper.Width, paper.TypeOfPaper.Density);
			return Ok(sheetCount);
		}

		private async Task SetViewData()
		{
			var formats = await _formatManager.GetAsync();
			var papers = await _paperManager.GetAsync();
			ViewData["FormatID"] = formats.Select(oRole => new SelectListItem { Text = oRole.Name, Value = oRole.FormatId.ToString() });
			ViewData["PaperID"] = papers.OrderBy(paper => paper.Name).Select(oRole => new SelectListItem { Text = oRole.Name, Value = oRole.PaperId.ToString() });

			var suppliersAndRecievers = _paperOrderManager.GetSuppliersAndRecievers();
			ViewData["RecieverCounterPartyID"] = suppliersAndRecievers
				.Where(rec => rec.PaperOrderRoleId == (long)PaperOrderRole.Reciever)
				.Select(oRole => new SelectListItem { Text = oRole.Name, Value = oRole.CounterpartyId.ToString() });

			var suppliers = suppliersAndRecievers
				.Where(rec => rec.PaperOrderRoleId == (long)PaperOrderRole.Supplier);
			ViewData["SupplierCounterPartyId"] = suppliers
				.Select(oRole => new SelectListItem { Text = oRole.Name, Value = oRole.CounterpartyId.ToString() });

			ViewData["Suppliers"] = JsonConvert.SerializeObject(suppliers.Select(supplier => new { supplier.CounterpartyId, supplier.PaymentDelay }));
		}
	}
}
