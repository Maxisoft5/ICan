using AutoMapper;
using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using ICan.Common.Utils;
using ICan.Data.Context;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class CalcManager : BaseManager
	{
		private readonly ProductManager _productManager;
		private readonly IWarehouseRepository _warehouseRepository;
		private readonly CommonManager<OptPaper> _paperManager;
		private readonly WarehouseJournalManager _whJournalManager;
		private readonly IPrintOrderRepository _printOrderRepository;
		private readonly IPrintOrderPaperRepository _printOrderPaperRepository;

		public CalcManager(IMapper mapper,
			IWarehouseRepository warehouseRepository, 
			ApplicationDbContext context, ILogger<BaseManager> logger,
			ProductManager productManager, WarehouseJournalManager whJournalManager,
			CommonManager<OptPaper> paperManager, IPrintOrderRepository printOrderRepository,
			IPrintOrderPaperRepository printOrderPaperRepository) : base(mapper, context, logger)
			
		{
			_productManager = productManager;
			_whJournalManager = whJournalManager;
			_paperManager = paperManager;
			_warehouseRepository = warehouseRepository;
			_printOrderRepository = printOrderRepository;
			_printOrderPaperRepository = printOrderPaperRepository;
		}

		public async Task<OptWarehouse> GetLatestWarehouseByTypeAsync(int whActionTypeId, int whObjectType, int? whType = null)
		{
			var wh = await _warehouseRepository.GetLatestWarehouseByTypeAsync(whActionTypeId, whObjectType, whType);
			return wh;
		}
		public (List<WarehouseJournalModel>, WarehouseModel) GetProductWarehouseState(int objectOd, DateTime? date, WarehouseType warehouseType)
		{
			var singleInventory = GetLatestSingleInventory(objectOd, date, warehouseType, WhJournalObjectType.Notebook);

			var rawList = _context.OptWarehouseJournal.Where(jItem =>
					jItem.ObjectId == objectOd &&
					(!date.HasValue || jItem.ActionDate >= date.Value)
					&& jItem.WarehouseTypeId == (int)warehouseType);
			var upds = _context.OptWarehouseJournal.Where(jItem =>
				   jItem.ObjectId == objectOd &&
				   jItem.ActionExtendedTypeId == (int)WhJournalActionExtendedType.UPD &&
					jItem.WarehouseTypeId == (int)warehouseType).ToList();
			if (singleInventory != null)
			{
				rawList = rawList.Where(jItem => jItem.ActionDate >= singleInventory.DateAdd);
			}
			var list = _mapper.Map<IEnumerable<WarehouseJournalModel>>(rawList).ToList();
			return (list, singleInventory);
		}

		public async Task<WarehouseModel> GetLatestInventoryAsync(WhJournalObjectType objectType, int? whType = null)
		{
			var raw = await GetLatestWarehouseByTypeAsync((int)WarehouseActionType.Inventory,
				(int)objectType, whType);
			var model = _mapper.Map<WarehouseModel>(raw);
			return model;
		}

		public IEnumerable<SelectListItem> GetActions(IEnumerable<int> availableActions = null, IEnumerable<int> excludedActions = null)
		{
			IQueryable<OptWarehouseActionType> actions = _context.OptWarehouseActionType;
			if (availableActions != null && availableActions.Any())
			{
				actions = actions.Where(action => availableActions.Contains(action.WarehouseActionTypeId));
			}
			if (excludedActions != null && excludedActions.Any())
			{
				actions = actions.Where(action => !excludedActions.Contains(action.WarehouseActionTypeId));
			}
			var selectList = actions
				.Select(action => new SelectListItem { Value = action.WarehouseActionTypeId.ToString(), Text = action.Name });
			return selectList;
		}


		public async Task<IEnumerable<CalcWhjDetails>> CalculateFromWhjAsync(Dictionary<OptProductseries, List<ProductModel>> productGroups)
		{
			productGroups ??= await _productManager
			  .GetAsync(false, dontShowDisabled: false, onlyNotebooks: false);
			var inventory = await GetLatestInventoryAsync(WhJournalObjectType.Notebook);
			var productIds = productGroups
				 .SelectMany(pgroup => pgroup.Value.Select(prgroupValue => (long)prgroupValue.ProductId));
			var journal = _whJournalManager.Get(inventory.DateAdd, productIds, WhJournalObjectType.Notebook, WarehouseType.NotebookReady);

			var calcDetailsList = new List<CalcWhjDetails>();

			foreach (var group in productGroups)
			{
				foreach (var product in group.Value)
				{
					var inventoryAmount = inventory.WarehouseItems
						.FirstOrDefault(inv => inv.ProductId == product.ProductId)?.Amount ?? 0;
					var singleInventory = GetLatestSingleInventory(product.ProductId, inventory?.DateAdd, WarehouseType.NotebookReady, WhJournalObjectType.Notebook);
					var journalProduct = journal.Where(j => j.ObjectId == product.ProductId);
					if (singleInventory != null)
					{
						journalProduct = journalProduct.Where(jItem => jItem.ActionDate >= singleInventory.DateAdd);
					}

					var calcWhjDetails = new CalcWhjDetails
					{
						ProductId = product.ProductId,
						Name = product.DisplayName,
						IsKit = product.IsKit,
						AssemblesAsKit = product.AssemblesAsKit,
						ProductSeriesId = product.ProductSeriesId,
						Journal = journalProduct,
						Inventory = inventoryAmount,
						InventoryDate = inventory?.DateAdd,
						SingleInventory = singleInventory?.WarehouseItems.First(whItem => whItem.ProductId == product.ProductId).Amount,
						SingleInventoryDate = singleInventory?.DateAdd
					};

					calcDetailsList.Add(calcWhjDetails);
				}
			}

			return calcDetailsList;
		}

		public async Task<IEnumerable<CalcPaperWhjDetails>> CalculatePaperFromWhjAsync(WarehouseType whType, IEnumerable<long> paperIds = null)
		{
			var inventory = await GetLatestInventoryAsync(WhJournalObjectType.Paper, (int)whType);
			var papers = await _paperManager.GetAsync();
			
			paperIds = (paperIds == null || !paperIds.Any())
			   ? papers.Select(paper => (long)paper.PaperId)
				: paperIds;
			var journal = _whJournalManager.Get(inventory?.DateAdd, paperIds, WhJournalObjectType.Paper, whType);
			var paperOrderIncomingIds = journal.Where(journalItem => journalItem.ActionExtendedTypeId == WhJournalActionExtendedType.PaperOrderIncoming)
				.Select(journalItem => int.Parse(journalItem.ActionId));
			var paperOrderIncomings = await _context.OptPaperOrderIncoming
					.Where(paperOrderInc => paperOrderIncomingIds.Contains(paperOrderInc.PaperOrderIncomingId))
					.ToListAsync();
			
			var printOrderPaperIds = journal.Where(journalItem => journalItem.ActionExtendedTypeId == WhJournalActionExtendedType.PrintOrder)
				.Select(journalItem => int.Parse(journalItem.ActionId));

			var printOrderPapersFromDB = await _context.OptPrintOrderPaper
					.Where(printOrderPaper => printOrderPaperIds.Contains(printOrderPaper.PrintOrderPaperId))
					.ToListAsync();

			var calcDetailsList = new List<CalcPaperWhjDetails>();
			foreach (var paper in papers.Where(paper=> paperIds.Contains(paper.PaperId)).OrderBy(paper => paper.Name))
			{
				var inventoryAmount = inventory?.WarehouseItems
						.FirstOrDefault(inv => inv.ObjectId == paper.PaperId)?.Amount ?? 0;
				var singleInventory = GetLatestSingleInventory(paper.PaperId, inventory?.DateAdd, whType, WhJournalObjectType.Paper);
				var journalPaper = journal.Where(j => j.ObjectId == paper.PaperId);
				

				if (singleInventory != null)
				{
					journalPaper = journalPaper
							.Where(jItem => jItem.ActionDate >= singleInventory.DateAdd)
							.OrderBy(journal => journal.ActionDate);
				}

				var calcWhjDetails = new CalcPaperWhjDetails
				{
					PaperId = paper.PaperId,
					PaperName = paper.Name,
					Journal = journalPaper,
					InventoryAmount = inventoryAmount,
					InventoryDate = inventory?.DateAdd,
					SingleInventoryAmount = singleInventory?.WarehouseItems.First(whItem => whItem.ObjectId == paper.PaperId).Amount,
					SingleInventoryDate = singleInventory?.DateAdd,
					WarehouseType = whType,
				};
				calcWhjDetails.PaperOrderIncomings = _mapper.Map<IEnumerable<PaperOrderIncomingModel>>(
					paperOrderIncomings.Where(paperInc => calcWhjDetails.PaperOrderIncomingItems.Select(item => int.Parse(item.ActionId)).
					Contains(paperInc.PaperOrderIncomingId)));				
				
				calcWhjDetails.PrintOrderPaperExtended = _mapper.Map<IEnumerable<PrintOrderPaperModel>>(

					printOrderPapersFromDB.Where(printOrderPaper => calcWhjDetails.PrintOrderPapers.Select(item => int.Parse(item.ActionId)).
						Contains(printOrderPaper.PrintOrderPaperId)));

				calcDetailsList.Add(calcWhjDetails);
			}

			return calcDetailsList;
		}

		public int CalcPrintingInOrders(long semiproductId, IEnumerable<OptPrintOrder> printOrders, List<PrintOrderRestAmountModel> printingOrdersInProgress)
		{
			int printingInProcess = 0;
			foreach (var printOrder in printOrders)
			{
				var printOrderSemiproducts = printOrder.PrintOrderSemiproducts.Where(pintOrdSem =>
							!pintOrdSem.IsAssembled &&
				pintOrdSem.SemiproductId == semiproductId);

				foreach (var printOrderSemiproduct in printOrderSemiproducts)
				{
					var printOrderIncomingsSum = printOrder.PrintOrderIncomings.SelectMany(t => t.PrintOrderIncomingItems).Where(t => t.PrintOrderSemiproductId == printOrderSemiproduct.PrintOrderSemiproductId)
				.Sum(incoming => incoming.Amount);
					var printingInProcessItem = Util.GetActingPrinting(printOrderSemiproduct.PrintOrder.Printing, (int)printOrderSemiproduct.SemiProduct.SemiproductTypeId) - printOrderIncomingsSum;

					if (printingInProcessItem > 0)
					{
						printingInProcess += printingInProcessItem;
						printingOrdersInProgress.Add(new PrintOrderRestAmountModel { PrintOrderId = printOrder.PrintOrderId, OrderDate = printOrder.OrderDate,
							PrintingHouseOrderNum = printOrder.PrintingHouseOrderNum,
							Printing = printOrder.Printing, RestAmount = printingInProcessItem });
					}
				}
			}
			return printingInProcess;
		}


		public WarehouseModel GetLatestSingleInventory(int objectId, DateTime? dateAdd,
			WarehouseType warehouseType, WhJournalObjectType objectType)
		{
			dateAdd ??= DateTime.MinValue;
			var singleInventory = _context.OptWarehouse
			   .Include(warehouse => warehouse.WarehouseItems)
			   .Where(warehouse => warehouse.DateAdd > dateAdd
				   && warehouse.WarehouseTypeId == (int)warehouseType
				   && warehouse.WarehouseActionTypeId == (int)WarehouseActionType.SingleInventory
				   && warehouse.WarehouseItems.Any(whItem => objectType == WhJournalObjectType.Paper ? whItem.ObjectId == objectId : whItem.ProductId == objectId))
				   .OrderByDescending(warehouse => warehouse.DateAdd)
				   .FirstOrDefault();

			return _mapper.Map<WarehouseModel>(singleInventory);
		}

		public bool CheckIsPaperExpensePlanCovered(OptPrintOrderPaper printOrderPaper)
        {
			if (printOrderPaper == null)
            {
				return false;
            }
			if (printOrderPaper.PrintOrder == null)
			{
				return false;
			}

			return printOrderPaper.PrintOrder.PaperPlannedExpense >= printOrderPaper.SheetsTakenAmount;
		}
      
	}
}
