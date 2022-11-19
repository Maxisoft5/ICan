using AutoMapper;
using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace ICan.Business.Managers
{
	public class MovePaperManager: BaseManager
	{
		private readonly IMovePaperRepository _movePaperRepository;
		private readonly IPrintOrderPaperRepository _printOrderPaperRepository;
		private readonly WarehouseJournalManager _whjManager;
		private readonly CalcManager _calcManager;
		private readonly WarehouseManager _warehouseManager;

		public MovePaperManager(IMapper mapper, ILogger<BaseManager> logger, IMovePaperRepository movePaperRepository,
			WarehouseJournalManager whjManager, CalcManager calcManager,
			IPrintOrderPaperRepository printOrderPaperRepository,
			WarehouseManager warehouseManager) : base(mapper, logger)
		{
			_movePaperRepository = movePaperRepository;
			_whjManager = whjManager;
			_calcManager = calcManager;
			_printOrderPaperRepository = printOrderPaperRepository;
			_warehouseManager = warehouseManager;
		}

		public async Task<IEnumerable<MovePaperModel>> GetMovings()
		{
			var movings = await _movePaperRepository.GetAsync();
			var mappedMovings = _mapper.Map<IEnumerable<MovePaperModel>>(movings);
			
			foreach (var move in mappedMovings)
            {
				var paperOrderPrint = await _printOrderPaperRepository.GetByIdAsync(move.PrintOrderPaperId.Value);
				if (paperOrderPrint == null || paperOrderPrint.PrintOrder == null 
					|| string.IsNullOrEmpty(paperOrderPrint.PrintOrder.PrintingHouseOrderNum))
				{
					continue;
				}
				move.IsExpensePlanCovered = _calcManager.CheckIsPaperExpensePlanCovered(paperOrderPrint);
				move.PrintOrderName = paperOrderPrint.PrintOrder.PrintingHouseOrderNum;

			}
			return mappedMovings;
		}

		public async Task<OptMovePaper> GetMovePaperByPrintPaperOrderId(int printPaperOrderId)
        {
			return await _movePaperRepository.GetByPrintOrderPaperId(printPaperOrderId);
		}

		public async Task<IEnumerable<OptPrintOrderPaper>> GetWithPrintOrderByPaperId(int paperId)
		{
			return await _printOrderPaperRepository.GetAllByPaperIdAsync(paperId);
		}

		public async Task Create(MovePaperModel model)
		{
			var paperWhState = (await _calcManager.CalculatePaperFromWhjAsync((WarehouseType)model.SenderWarehouseId, new List<long> { model.PaperId })).First();

            if (paperWhState.Current < model.SheetCount)
                throw new UserException("Недостаточно бумаги для отправки");

			var optMovePaper = _mapper.Map<OptMovePaper>(model);

			var whjOutcome = new WarehouseJournalModel
			{
				ActionDate = model.MoveDate,
				ObjectTypeId = WhJournalObjectType.Paper,
				ActionTypeId = WhJournalActionType.Outcome,
				Amount = model.SheetCount,
				WarehouseTypeId = (WarehouseType)model.SenderWarehouseId,
				ActionExtendedTypeId = WhJournalActionExtendedType.MovingPaper,
				ObjectId = model.PaperId,
				Comment = model.Comment				
			};

			var whjIncome = new WarehouseJournalModel
			{
				ActionDate = model.MoveDate,
				ObjectTypeId = WhJournalObjectType.Paper,
				ActionTypeId = WhJournalActionType.Income,
				Amount = model.SheetCount,
				WarehouseTypeId = (WarehouseType)model.ReceiverWarehouseId,
				ActionExtendedTypeId = WhJournalActionExtendedType.MovingPaper,
				ObjectId = model.PaperId,
				Comment = model.Comment
			};

			try
			{
				using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
				var movePaperId = (await _movePaperRepository.AddAsync(optMovePaper)).ToString();
				whjOutcome.ActionId = movePaperId;
				whjIncome.ActionId = movePaperId;
				await _whjManager.AddRangeAsync(new List<WarehouseJournalModel> { whjIncome, whjOutcome });
				transaction.Complete();
			}
			catch(TransactionAbortedException ex)
			{
				_logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task CreateAutoMovePaper(int printOrderPaperId)
        {
			var optMovePaper = await GetMovePaperByPrintPaperOrderId(printOrderPaperId);
			if (optMovePaper != null)
            {
				_logger.LogWarning($"Перемещение бумаги с printOrderPaperId:{printOrderPaperId} уже создано");
				return;
			}
			var optPrintOrder = await _printOrderPaperRepository.GetByIdAsync(printOrderPaperId);
			var senderWh = await _warehouseManager.GetWareHouseByType(WarehouseType.PaperReady);
			var receiverWh = await _warehouseManager.GetWareHouseByType(WarehouseType.PaperReadyZetaPrint);
			var movePaper = new MovePaperModel()
			{
				MoveDate = DateTime.Now,
				PaperName = optPrintOrder.PaperOrder.Paper.Name,
				PrintOrderName = optPrintOrder.PrintOrder.PrintingHouseOrderNum,
				ReceiverName = receiverWh.Name,
				SenderName = senderWh.Name,
				PaperId = optPrintOrder.PaperOrder.PaperId,
				Comment= "Создано автоматически",
				SheetCount = optPrintOrder.SheetsTakenAmount,
				SenderWarehouseId = senderWh.WarehouseTypeId,
				ReceiverWarehouseId = receiverWh.WarehouseTypeId,
				Weight = PaperOrderModel.GetWeight(optPrintOrder.SheetsTakenAmount, optPrintOrder.PaperOrder.Paper.Length,
					optPrintOrder.PaperOrder.Paper.Width, optPrintOrder.PaperOrder.Paper.TypeOfPaper.Density),
				PrintOrderPaperId = printOrderPaperId
			};
			await Create(movePaper);

        }

		public async Task Delete(int id)
		{
			if (id == 0)
				throw new Exception($"Не найдена запись с указанным id = {id}");

			try
			{
				using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
				await _movePaperRepository.DeleteAsync(id);
				await _whjManager.RemoveByAction(id.ToString(), (int)WhJournalActionExtendedType.MovingPaper);
				transaction.Complete();
			}
			catch(TransactionAbortedException ex)
			{
				_logger.LogError(ex.Message);
				throw;
			}
		}

		public async Task<bool> DeleteAutoPaper(int printOrderPaperId)
        {
			var movePaper = await _movePaperRepository.GetByPrintOrderPaperId(printOrderPaperId);
			if (movePaper == null)
            {
				_logger.LogWarning($"Перемещение бумаги с printOrderPaperId: {printOrderPaperId} уже удалён");
				return false;
            }
			await Delete(movePaper.MovePaperId);
			return true;
		}

	}
}
