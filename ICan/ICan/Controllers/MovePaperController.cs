using ICan.Business.Managers;
using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Controllers
{
	[ServiceFilter(typeof(SetRoleAttribute))]
	public class MovePaperController: BaseController
	{
		private readonly MovePaperManager _movePaperManager;
		private readonly PaperWarehouseManager _paperWhManager;
		private readonly PrintOrderManager _printOrderManager;
		private readonly CommonManager<OptPaper> _paperManager;

		public MovePaperController(MovePaperManager movePaperManager, ILogger<BaseController> logger,
			PaperWarehouseManager paperWarehouseManager, CommonManager<OptPaper> paperManager, PrintOrderManager printOrderManager) : base(logger)
		{
			_movePaperManager = movePaperManager;
			_paperWhManager = paperWarehouseManager;
			_paperManager = paperManager;
			_printOrderManager = printOrderManager;
		}

		public async Task<IActionResult> Index()
		{
			var movings = await _movePaperManager.GetMovings();
			return View(movings.OrderByDescending(x => x.MoveDate));
		}

		public async Task<IActionResult> Create()
		{
			var movePaper = new MovePaperModel {
				MoveDate = DateTime.Now
			};
			await SetViewData();
			return View(movePaper);
		}

		private async Task SetViewData()
		{
			ViewBag.PaperWhs = _paperWhManager.GetWarehouseTypesList();
			var papers = await _paperManager.GetAsync();
			ViewData["PaperList"] = papers.OrderBy(paper => paper.Name).Select(paper => new SelectListItem { Text = paper.Name, Value = paper.PaperId.ToString() });
		}

		[HttpPost]
		public async Task<IActionResult> Create(MovePaperModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await _movePaperManager.Create(model);
					TempData["StatusMessage"] = Const.SuccessMessages.Saved;
				}
				catch (UserException ex)
				{
					TempData["ErrorMessage"] = ex.Message;
					await SetViewData();
					return View(model);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex.Message);
					TempData["ErrorMessage"] = Const.ErrorMessages.CantSaveForUser;
				}
			}
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public async Task<IActionResult> CreateAutoMovePaper(int printOrderPaperId)
        {
			await _movePaperManager.CreateAutoMovePaper(printOrderPaperId);
			return Ok();
		}

		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _movePaperManager.Delete(id);
				TempData["StatusMessage"] = Const.SuccessMessages.Deleted;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message);
				TempData["ErrorMessage"] = Const.ErrorMessages.CantDelete;
			}
			return RedirectToAction(nameof(Index));
		}

		[HttpDelete]
		public async Task<IActionResult> DeleteAutoMovePaper(int printOrderPaperId)
        {
			if(await _movePaperManager.DeleteAutoPaper(printOrderPaperId))
            {
				return Ok("Удален");
			}
			return Ok("Уже удален");
        }
			

		[HttpPost]
        public async Task<IActionResult> GetPrintOrdersByPaperId(int paperId)
        {
			var printOrdersPapers = await _movePaperManager.GetWithPrintOrderByPaperId(paperId);
			
			return Json(new { PrintOrders = printOrdersPapers.Select(x => new { Value = x.PrintOrderPaperId.ToString(), Text = x.PrintOrder.PrintingHouseOrderNum }) });
		}



	}
}
