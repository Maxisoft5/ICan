using AutoMapper;
using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class WarehouseJournalManager : BaseManager
	{
		private static Lazy<Dictionary<WarehouseActionType, WhJournalActionExtendedType>> extendedActionTypeMapping
		= new Lazy<Dictionary<WarehouseActionType, WhJournalActionExtendedType>>(() =>
		   new Dictionary<WarehouseActionType, WhJournalActionExtendedType>()
		   {
				{ WarehouseActionType.Arrival, WhJournalActionExtendedType.WarehouseArrival },
				{ WarehouseActionType.Marketing, WhJournalActionExtendedType.Marketing },
				{ WarehouseActionType.Returning, WhJournalActionExtendedType.Returning },
				{ WarehouseActionType.KitAssembly, WhJournalActionExtendedType.KitAssembly },
		   }
		);

		private readonly IWarehouseJournalRepository _whjRepository;
		private readonly IPrintOrderRepository _printOrderRepository;

		public WarehouseJournalManager(IMapper mapper, ILogger<BaseManager> logger,
			IWarehouseJournalRepository whjRepository, IPrintOrderRepository printOrderRepository) : base(mapper, logger)
		{
			_whjRepository = whjRepository;
			_printOrderRepository = printOrderRepository;
		}

		public IEnumerable<WarehouseJournalModel> Get(DateTime? date, IEnumerable<long> objectIds, WhJournalObjectType whJournalObjectType, WarehouseType whType)
		{
			var rawList = _whjRepository.GetByParams(date, objectIds, whJournalObjectType, whType);
			var list = _mapper.Map<IEnumerable<WarehouseJournalModel>>(rawList);
			return list;
		}

		public IEnumerable<WarehouseJournalModel> Get(DateTime? date, WarehouseType whType)
		{
			var rawList = _whjRepository.GetByParams(date, whType);
			var list = _mapper.Map<IEnumerable<WarehouseJournalModel>>(rawList);
			return list;
		}

		public async Task AddAsync(WarehouseJournalModel model, bool needSave = true)
		{
			var raw = _mapper.Map<OptWarehouseJournal>(model);
			await _whjRepository.Add(raw, needSave);
		}

		public async Task AddRangeAsync(IEnumerable<WarehouseJournalModel> list, bool needSave = true)
		{
			var rawList = _mapper.Map<IEnumerable<OptWarehouseJournal>>(list);
			await _whjRepository.AddRange(rawList, needSave);
		}

		public async Task RemoveByAction(OptWarehouse whouse, bool needSave = true)
		{
			var parsedAction = Enum.Parse<WarehouseActionType>(whouse.WarehouseActionTypeId.ToString());
			if (parsedAction == WarehouseActionType.Inventory || parsedAction == WarehouseActionType.SingleInventory)
			{
				return;
			}
			var actionExtendedType = extendedActionTypeMapping.Value[parsedAction];
			var rawList = _whjRepository.GetByAction(whouse.WarehouseId.ToString(), (int)actionExtendedType);				
			await _whjRepository.RemoveRange(rawList, needSave);
			if (actionExtendedType == WhJournalActionExtendedType.KitAssembly)
			{

				var kitAssemblyPartEntries = _whjRepository.GetByAction(whouse.WarehouseId.ToString(), (int)WhJournalActionExtendedType.KitAssemblyPart); 
				await _whjRepository.RemoveRange(kitAssemblyPartEntries, needSave);
			}
		}

		public async Task RemoveByAction(OptPrintOrder printOrder, bool needSave = true)
		{
			var printOrderFromDb = await _printOrderRepository.GetByIdAsync(printOrder.PrintOrderId);
			// remove papers outcome 
			var printOrderPapers = printOrderFromDb.PrintOrderPapers;
			await RemoveByAction(printOrderPapers, needSave);

			// remove  semiproducts income 
			var printOrderIncomings = printOrderFromDb.PrintOrderIncomings
				 ?.Select(incomning => incomning.PrintOrderIncomingId.ToString())
				 .ToList();
			if (printOrderIncomings.Any())
			{
				var semiproductTypeId = printOrderFromDb
					.PrintOrderSemiproducts?.FirstOrDefault().SemiProduct.SemiproductTypeId;
				var warehouseType = semiproductTypeId == (int)SemiProductType.Stickers
						? WarehouseType.StickersUnNotched
						: WarehouseType.SemiproductReady;

				var whJournalsByActionIds = _whjRepository.GetByActionIds(printOrderIncomings);
				var rawList = whJournalsByActionIds.Where(whj => whj.ActionTypeId == (int)WhJournalActionType.Income
					&& whj.ActionExtendedTypeId == (int)WhJournalActionExtendedType.PrintOrder
					&& whj.WarehouseTypeId == (int)warehouseType
					&& whj.ObjectTypeId == (int)WhJournalObjectType.Semiproduct
				  );
				await _whjRepository.RemoveRange(rawList, needSave);
			}
		}

		public async Task RemoveByAction(ICollection<OptPrintOrderPaper> printOrderPapers, bool needSave = true)
		{
			if (printOrderPapers != null && printOrderPapers.Any())
			{
				var actions = printOrderPapers.Select(printOrderPaper => printOrderPaper.PrintOrderPaperId.ToString())
				 .ToList();
				var whJournalsByActionIds = _whjRepository.GetByActionIds(actions);
				var rawList = whJournalsByActionIds
					.Where(whj => whj.ActionTypeId == (int)WhJournalActionType.Outcome
					&& whj.ActionExtendedTypeId == (int)WhJournalActionExtendedType.PrintOrder
					&& whj.ObjectTypeId == (int)WhJournalObjectType.Paper
				  );

				await _whjRepository.RemoveRange(rawList, needSave);
			}
		}

		public async Task RemoveByAction(IEnumerable<OptReport> oldReports, bool needSave = true)
		{
			var reportIds = oldReports.Select(report => report.ReportId).ToList();
			var whJournalsByAction = _whjRepository.GetByActionIds(reportIds);
			var rawList = whJournalsByAction
				.Where(whj => whj.ActionExtendedTypeId == (int)WhJournalActionExtendedType.UPD);
			await _whjRepository.RemoveRange(rawList, needSave);
		}


		public async Task RemoveByAction(OptAssembly assembly, bool needSave = true)
		{
			var whJournals = _whjRepository.GetByAction(assembly.AssemblyId.ToString(), (int)WhJournalActionExtendedType.AssemblyPart);
			var rawList = whJournals.Where(whj => whj.ActionTypeId == (int)WhJournalActionType.Outcome
				&& whj.ObjectTypeId == (int)WhJournalObjectType.Semiproduct);
			await _whjRepository.RemoveRange(rawList, needSave);
		}

		public async Task UpdateJournal(AssemblyModel model, bool needSave = true)
		{
			var whJournals = _whjRepository.GetByAction(model.AssemblyId.ToString(), (int)WhJournalActionExtendedType.AssemblyPart);
			var whJournalRaws = whJournals.Where(x =>  x.ActionTypeId == (int)WhJournalActionType.Outcome
				&& x.ObjectTypeId == (int)WhJournalObjectType.Semiproduct).ToList();

			whJournalRaws.ForEach(x =>
			{
				x.Amount = model.Amount;
			});
			await _whjRepository.UpdateRange(whJournalRaws, needSave);
		}
		
		public async Task UpdateJournal(GluePadIncomingModel model, bool needSave = true)
		{
			var whJournals = _whjRepository.GetByAction(model.Id.ToString(), (int)WhJournalActionExtendedType.WarehouseArrival);
			var whJournal = whJournals.First(x => x.ActionTypeId == (int)WhJournalActionType.Income
				&& x.ObjectTypeId == (int)WhJournalObjectType.GluePad
				&& x.ObjectId == Const.GluePadProductId);

			whJournal.Amount = model.Amount;
			await _whjRepository.Update(whJournal, needSave);
		}

		public async Task RemoveByAction(string actionId, int actionExtendedTypeId, bool needSave = true)
		{
			await _whjRepository.RemoveRangeByAction(actionId, actionExtendedTypeId, needSave);
		}

		public async Task RemoveByAction(IEnumerable<OptNotchOrderIncoming> notchOrderIncoming, bool needSave = true)
		{
			var incomingIds = notchOrderIncoming.Select(x => x.NotchOrderIncomingId.ToString());
			var whJournals = _whjRepository.GetByActionIds(incomingIds);
			var rawList = whJournals.Where(whj => whj.ActionExtendedTypeId == (int)WhJournalActionExtendedType.NotchOrderIncoming
				&& whj.ObjectTypeId == (int)WhJournalObjectType.Semiproduct);
			await _whjRepository.RemoveRange(rawList, needSave);
		}

		public static int GetSubState(IEnumerable<OptWarehouseJournal> journal, int warehouseType)
		{
			var income = journal.Where(whj =>
				whj.WarehouseTypeId == warehouseType
				&& whj.ActionTypeId == (int)WhJournalActionType.Income)
				.Sum(whj => whj.Amount);
			var outCome = journal.Where(whj =>
				whj.WarehouseTypeId == warehouseType
				&& whj.ActionTypeId == (int)WhJournalActionType.Outcome)
				.Sum(whj => whj.Amount);
			var result = income - outCome;
			return result;
		}

		public async Task RemoveByAction(IEnumerable<OptPaperOrderIncoming> paperOrderIncomings, bool needSave = true)
		{
			var incomingIds = paperOrderIncomings.Select(x => x.PaperOrderIncomingId.ToString());
			var whJournals = _whjRepository.GetByActionIds(incomingIds);
			var rawList = whJournals.Where(whj => whj.ActionExtendedTypeId == (int)WhJournalActionExtendedType.PaperOrderIncoming
				&& whj.ObjectTypeId == (int)WhJournalObjectType.Paper);
			await _whjRepository.RemoveRange(rawList, needSave);
		}
	}
}