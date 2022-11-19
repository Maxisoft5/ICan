using AutoMapper;
using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Common.Models.Opt.Report;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class PrintOrderManager : BaseManager
	{
		private readonly WarehouseJournalManager _whJournalManager;
		private readonly PaperOrderManager _paperOrderManager;
		private readonly IPrintOrderRepository _printOrderRepository;
		private readonly IMovePaperRepository _movePaperRepository;

		public PrintOrderManager(IMapper mapper, IPrintOrderRepository printOrderRepository, IMovePaperRepository movePaperRepository,
ApplicationDbContext context, ILogger<BaseManager> logger,
			WarehouseJournalManager whJournalManager, IConfiguration configuration, PaperOrderManager paperOrderManager)
			: base(mapper, context, logger, configuration)
		{
			_whJournalManager = whJournalManager;
			_paperOrderManager = paperOrderManager;
			_printOrderRepository = printOrderRepository;
			_movePaperRepository = movePaperRepository;
		}

		public async Task CreateAsync(PrintOrderModel model)
		{
			OptPrintOrder printOrder = null;
			var journal = new List<WarehouseJournalModel>();
			try
			{
				var paperOrders = _paperOrderManager.Get()
								.Where(x => model.PrintOrderPapers
											.Select(modelPaperOrder => modelPaperOrder.PaperOrderId)
											.Contains(x.PaperOrderId))
								.ToList();

				foreach (var item in model.PrintOrderPapers)
				{
					var paperOrder = paperOrders.First(x => x.PaperOrderId == item.PaperOrderId);

					if (paperOrder.SheetsLeftAmount < item.SheetsTakenAmount)
						throw new UserException($"Недостаточно бумаги в заказе от {paperOrder.OrderDate.ToShortDateString()}");
				}

				printOrder = await _printOrderRepository.CreateAsync(model);
				await SpendPaper(model, printOrder.PrintOrderPapers, journal);
			}
			catch
			{
				await CleanDataAsync(printOrder);
				throw;
			}
		}

		private async Task SpendPaper(PrintOrderModel model, IEnumerable<OptPrintOrderPaper> printOrderPapers, List<WarehouseJournalModel> journal)
		{
			printOrderPapers.Where(paper => paper.IsSent)
				.ToList().ForEach(printOrderPaper =>
								journal.Add(new WarehouseJournalModel
								{
									ActionDate = model.OrderDate,
									ActionTypeId = WhJournalActionType.Outcome,
									ActionExtendedTypeId = WhJournalActionExtendedType.PrintOrder,
									ObjectTypeId = WhJournalObjectType.Paper,
									ObjectId = printOrderPaper.PaperOrder.PaperId,
									Amount = printOrderPaper.SheetsTakenAmount,
									ActionId = printOrderPaper.PrintOrderPaperId.ToString(),
									WarehouseTypeId = WarehouseType.PaperReadyZetaPrint,
								}));

			await _whJournalManager.AddRangeAsync(journal);
		}

		public IEnumerable<PrintOrderModel> Get()
		{
			var rawList = _printOrderRepository.Get().Where(x => x.IsArchived == false);
			var list = _mapper.Map<IEnumerable<PrintOrderModel>>(rawList);
			return list;
		}


		public async Task EditAsync(PrintOrderModel model, ClaimsPrincipal user)
		{
			var raw = await _printOrderRepository.GetByIdAsync(model.PrintOrderId);

			if (model.IsPaid)
			{
				CheckPayments(model.PrintOrderId, pr =>
				{
					throw new UserException("Невозможно сохранить информацию, невозможно проставить флаг \"Оплачен\", если сумма платежей меньше, чем Сумма заказа");
				});
			}
			try
			{
				var journal = new List<WarehouseJournalModel>();
				var printOrderPapers = new List<OptPrintOrderPaper>();

				if (!user.IsInRole(Const.Roles.Designer))
				{
					raw.OrderDate = model.OrderDate;
					raw.PrintingHouseOrderNum = model.PrintingHouseOrderNum;
					raw.Printing = model.Printing;
					raw.OrderSum = model.OrderSum;
					raw.PaymentDate = model.PaymentDate;
					raw.IsPaid = model.IsPaid;
					raw.IsAssembled = model.IsAssembled;
					raw.CheckNumber = model.CheckNumber;
					raw.Comment = model.Comment;
					raw.PaperPlannedExpense = model.PaperPlannedExpense;

					await _whJournalManager.RemoveByAction(raw.PrintOrderPapers, false);
					_context.RemoveRange(raw.PrintOrderPapers);

					var paperOrders = _paperOrderManager.Get()
								.Where(x => model.PrintOrderPapers.Select(y => y.PaperOrderId)
								.Contains(x.PaperOrderId))
								.ToList();

					foreach (var item in model.PrintOrderPapers)
					{
						var movePaper = await _movePaperRepository.GetByPrintOrderPaperId(item.PrintOrderPaperId);
						var paperOrder = paperOrders.First(x => x.PaperOrderId == item.PaperOrderId);
						var sheetsTakenAmountOldValue = raw.PrintOrderPapers.FirstOrDefault(x => x.PrintOrderPaperId == item.PrintOrderPaperId)?.SheetsTakenAmount ?? 0;
						if (paperOrder.SheetsLeftAmount + sheetsTakenAmountOldValue < item.SheetsTakenAmount)
							throw new UserException($"Недостаточно бумаги в заказе от {paperOrder.OrderDate.ToShortDateString()}");

						var printOrderPaper = new OptPrintOrderPaper
						{
							PrintOrder = raw,
							PaperOrderId = item.PaperOrderId,
							SheetsTakenAmount = item.SheetsTakenAmount,
							IsSent = item.IsSent
						};
						await _context.AddAsync(printOrderPaper);
						await _context.SaveChangesAsync();
						movePaper.PrintOrderPaperId = printOrderPaper.PrintOrderPaperId;
						await _context.AddAsync(movePaper);
						await _context.SaveChangesAsync();
						printOrderPapers.Add(printOrderPaper);
					}
					var modelPrintOrderSemiProductIds = model.PrintOrderSemiproducts.Select(semiP => semiP.PrintOrderSemiproductId);
					var dbPrintOrderSemiProductIds = raw.PrintOrderSemiproducts.Select(semiP => semiP.PrintOrderSemiproductId);

					//удалить те, которых нет в модели
					var itemsToRemove = raw.PrintOrderSemiproducts.Where(semiP => !modelPrintOrderSemiProductIds.Contains(semiP.PrintOrderSemiproductId));
					var itemsToRemoveIds = itemsToRemove.Select(printOrderSemiP => printOrderSemiP.PrintOrderSemiproductId);
					if (raw.PrintOrderIncomings == null || !raw.PrintOrderIncomings.Any())
					{
						var errors = await CheckCanRemoveItems(itemsToRemoveIds, model.PrintOrderId);
						if (!string.IsNullOrWhiteSpace(errors))
						{
							throw new UserException($"Невозможно внести изменения. Обнаружены следующие препятствия: {errors}");
						}
						//добавить те, которые есть в модели, но нет в БД 
						var itemsToAdd = model.PrintOrderSemiproducts.Where(semiP => !dbPrintOrderSemiProductIds.Contains(semiP.PrintOrderSemiproductId)).ToList();

						var dbItemsToAdd = _mapper.Map<List<OptPrintOrderSemiproduct>>(itemsToAdd);
						dbItemsToAdd.ForEach(x => { x.PrintOrder = raw; });

						_context.RemoveRange(itemsToRemove);
						await _context.AddRangeAsync(dbItemsToAdd);
					}

					foreach (var dbSemiproduct in raw.PrintOrderSemiproducts.Where(smProd => !itemsToRemoveIds.Contains(smProd.PrintOrderSemiproductId)))
					{
						dbSemiproduct.IsAssembled = model.PrintOrderSemiproducts.First(semiP => dbSemiproduct.PrintOrderSemiproductId == semiP.PrintOrderSemiproductId).IsAssembled;
					}
				}


				raw.ConfirmPrint = model.ConfirmPrint;

				await _context.SaveChangesAsync();
				await SpendPaper(model, printOrderPapers, journal);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[PrintOrder][Edit] erro occured while editing print order, data can be inconsistendt ");
				throw;
			}
		}

		public async Task<OptPrintOrder> GetRawAsync(int value)
		{
			var raw = await _printOrderRepository.GetByIdAsync(value);
			return raw;
		}

		public async Task<PrintOrderModel> GetAsync(int value)
		{
			var raw = await GetRawAsync(value);
			var model = _mapper.Map<PrintOrderModel>(raw);
			return model;
		}

		public async Task<IEnumerable<dynamic>> GetPrintOrdersByPaperId(int paperId)
        {
			var printOrders = await _printOrderRepository.GetByPaperIdAsync(paperId);

			var mappedOrders = _mapper.Map<IEnumerable<PrintOrderModel>>(printOrders);
			
			return mappedOrders;
		} 

		public async Task DeleteAsync(int id)
		{
			var raw = await _context.OptPrintOrder.Include(x => x.PrintOrderSemiproducts).FirstOrDefaultAsync(m => m.PrintOrderId == id);
			var printOrderSemiproductIds = raw.PrintOrderSemiproducts.Select(x => x.PrintOrderSemiproductId);
			var errors = await CheckCanRemoveItems(printOrderSemiproductIds, id);
			if (!string.IsNullOrWhiteSpace(errors))
			{
				throw new UserException($"Невозможно удалить заказ печати. Обнаружены следующие препятствия: {errors}");
			}
			await CleanDataAsync(raw);
			await _context.SaveChangesAsync();
		}



		public bool UsedInAssemblies(int printOrderId)
		{
			var printOrderSemiproducts = _context.OptPrintOrderSemiproduct.Where(printOrderS => printOrderS.PrintOrderId == printOrderId).Select(printOrderS => printOrderS.PrintOrderSemiproductId).ToList();
			var usedInAssemblies = _context.OptAssemblySemiproduct.Any(assemblySP =>
			assemblySP.PrintOrderSemiproductId.HasValue &&
				printOrderSemiproducts.Contains(assemblySP.PrintOrderSemiproductId.Value));

			return usedInAssemblies;
		}

		public bool UsedInNotchOrders(int id)
		{
			var usedInNotchOrders = _context.OptNotchOrderItem.Any(notchOrder => notchOrder.PrintOrderId == id);
			if (usedInNotchOrders)
				return true;
			return UsedInAssemblies(id);
		}

		public async Task<IEnumerable<PrintOrderIncomingModel>> GetPrintOrderIncomingsAsync(int printOrderId)
		{
			var model = await GetAsync(printOrderId);
			SetSemiproducts(model);
			SetPrintOrderIncomings(model);
			return model.PrintOrderIncomings;
		}

		public async Task<IEnumerable<PrintOrderPaymentModel>> GetPrintOrderPaymentsAsync(int printOrderId)
		{
			var model = await GetAsync(printOrderId);
			return model.PrintOrderPayments;
		}

		public void SetSemiproducts(PrintOrderModel model)
		{
			if (model.PrintOrderSemiproducts != null)
			{
				var semiProductIds = model.PrintOrderSemiproducts.Select(sProd => sProd.SemiproductId);

				var semiproducts = _mapper.Map<IEnumerable<SemiproductModel>>(_context.OptSemiproduct
					.Include(sProd => sProd.SemiproductType)
					.Include(sProd => sProd.Product)
						.ThenInclude(prod => prod.ProductSeries)
					.Include(sProd => sProd.Product)
						.ThenInclude(prod => prod.Country)
					.Where(sProd => semiProductIds.Contains(sProd.SemiproductId)));

				model.PrintOrderSemiproducts
					.ForEach(printOrderSemiProdict =>
					printOrderSemiProdict.SemiProduct = semiproducts.First(semi => semi.SemiproductId == printOrderSemiProdict.SemiproductId));
			}
		}

		public void SetPrintOrderIncomings(PrintOrderModel model)
		{
			if (model.PrintOrderIncomings != null && model.PrintOrderIncomings.Any())
			{
				model.PrintOrderIncomings.ForEach(inco =>
					inco.PrintOrderIncomingItems.ForEach(incomingItem =>
					incomingItem.PrintOrderSemiproductName = model.PrintOrderSemiproducts
						.First(s => s.PrintOrderSemiproductId == incomingItem.PrintOrderSemiproductId).SemiProduct.DisplayName));
			}
		}

		public async Task<List<OptPrintOrderSemiproduct>> GetPrintOrdersBySemiproductId(long semiproductId)
		{
			var printOrders = await _context.OptPrintOrderSemiproduct
					.Where(x => x.SemiproductId == semiproductId && (x.PrintOrder.IsAssembled == null || x.PrintOrder.IsAssembled == false))
					.Include(x => x.PrintOrder)
						.OrderByDescending(x => x.PrintOrder.OrderDate)
					.Include(x => x.SemiProduct)
						.ThenInclude(x => x.SemiproductType)
					.ToListAsync();

			return printOrders;
		}

		public async Task AddIncoming(PrintOrderIncomingModel incoming)
		{
			using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
			{
				try
				{
					var raw = new OptPrintOrderIncoming
					{
						IncomingDate = incoming.IncomingDate,
						PrintOrderId = incoming.PrintOrderId,
						IncomingType = (int)incoming.IncomingType,
						Comment = incoming.Comment
					};

					await _context.AddAsync(raw);
					await _context.SaveChangesAsync();
					var printOrder = _context.OptPrintOrder
						.Include(pOrder => pOrder.PrintOrderSemiproducts)
							.ThenInclude(pOrder => pOrder.SemiProduct)
						.FirstOrDefault(pOrder => pOrder.PrintOrderId == incoming.PrintOrderId);
					var semiproductTypeId = printOrder?.PrintOrderSemiproducts?.FirstOrDefault()
						.SemiProduct.SemiproductTypeId;

					var warehouseType = semiproductTypeId == (int)SemiProductType.Stickers
						? WarehouseType.StickersUnNotched
						: WarehouseType.SemiproductReady;
					var whActionType = incoming.IncomingType == PrintOrderIncomingType.Flaw ? WhJournalActionType.Outcome
						: WhJournalActionType.Income;

					var whJornalList = new List<WarehouseJournalModel>();
					foreach (var item in incoming.PrintOrderIncomingItems.Where(item => item.Amount > 0))
					{
						var rawItem = new OptPrintOrderIncomingItem
						{
							Amount = item.Amount,
							PrintOrderSemiproductId = item.PrintOrderSemiproductId,
							PrintOrderIncoming = raw
						};
						await _context.AddAsync(rawItem);
						whJornalList.Add(new WarehouseJournalModel
						{
							ActionDate = incoming.IncomingDate,
							ActionId = raw.PrintOrderIncomingId.ToString(),
							ActionTypeId = whActionType,
							ActionExtendedTypeId = WhJournalActionExtendedType.PrintOrder,
							ObjectTypeId = WhJournalObjectType.Semiproduct,
							ObjectId = GetSemiproductIdByPrintOrderSemiproductId(item.PrintOrderSemiproductId),
							Amount = item.Amount,
							WarehouseTypeId = warehouseType,
							Comment = $"Дата заказа печати: { printOrder.OrderDate}, тираж заказа печати: { printOrder.Printing},  дата прихода: { incoming.IncomingDate}, тип прихода: {incoming.IncomingTypeName},  количество { item.Amount}"
						});
					}
					await _whJournalManager.AddRangeAsync(whJornalList);
					await _context.SaveChangesAsync();
					await transaction.CommitAsync();
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"[PrintOrder][AddIncoming]");
					await transaction.RollbackAsync();
					throw;
				}
			}
		}

		public async Task<PrintOrderModel> GetReportAsync(int id)
		{
			var printOrder = await _context.OptPrintOrder
									.Include(x => x.PrintOrderSemiproducts)
										.ThenInclude(x => x.SemiProduct)
										.ThenInclude(x => x.SemiproductType)
									.Include(x => x.PrintOrderSemiproducts)
										.ThenInclude(x => x.SemiProduct)
										.ThenInclude(x => x.Product)
										.ThenInclude(x => x.ProductSeries)
									.Include(x => x.PrintOrderSemiproducts)
										.ThenInclude(x => x.SemiProduct)
										.ThenInclude(x => x.Product)
										.ThenInclude(x => x.Country)
									.Where(x => x.PrintOrderId == id)
									.FirstOrDefaultAsync();
			var result = _mapper.Map<PrintOrderModel>(printOrder);

			if (result.SemiProductTypeId != (int)SemiProductType.Stickers)
			{
				var assemblies = _context.OptAssembly
										.Include(x => x.AssemblySemiproducts)
										.Where(x => x.AssemblySemiproducts.Any(a =>
											result.PrintOrderSemiproducts.Select(x => x.PrintOrderSemiproductId).ToList().Contains((int)a.PrintOrderSemiproductId)));
				result.Assemblies = _mapper.Map<IEnumerable<AssemblyModel>>(assemblies);
			}
			else
			{
				var notchOrders = _context.OptNotchOrder
										.Include(x => x.NotchOrderItems)
										.Where(x => x.NotchOrderItems.Select(x => x.PrintOrderId).Contains(id));
				result.NotchOrders = _mapper.Map<IEnumerable<NotchOrderModel>>(notchOrders);
			}
			return result;
		}

		public long GetSemiproductIdByPrintOrderSemiproductId(long printOrderSemiproductId)
		{
			var printOrderSemiproduct = _context.OptPrintOrderSemiproduct.FirstOrDefault(x => x.PrintOrderSemiproductId == printOrderSemiproductId);
			return printOrderSemiproduct.SemiproductId;
		}

		public async Task DeleteIncoming(int id)
		{
			var raw = await _context.OptPrintOrderIncoming.FindAsync(id);
			if (raw == null)
				throw new UserException("Приход не найден");
			var thisOrderSemiproducts = _context.OptPrintOrderSemiproduct.Where(printorderSP => printorderSP.PrintOrderId == raw.PrintOrderId);

			var assemblies = _context
				.OptAssemblySemiproduct
				.Include(assemblProd => assemblProd.Assembly)
				.Join(thisOrderSemiproducts, assembl => assembl.PrintOrderSemiproductId, printorder => printorder.PrintOrderSemiproductId, (assembl, printOrder) => assembl.Assembly);
			if (assemblies.Count() > 0)
			{
				var errorStr = string.Join(",", assemblies.Select(assembl => assembl.Date).Distinct());
				throw new UserException($"Невозможно удалить приход, так как заказ печати участвуеет в сборках от {errorStr}");
			}

			await _whJournalManager.RemoveByAction(id.ToString(), (int)WhJournalActionExtendedType.PrintOrder);
			_context.Remove(raw);
			await _context.SaveChangesAsync();
		}

		public async Task<int?> DeletePayment(int id)
		{
			using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
			{
				try
				{
					var raw = await _context.OptPrintOrderPayment
					.FirstOrDefaultAsync(payment => payment.PrintOrderPaymentId == id);
					if (raw == null)
						return null;
					var printOrderId = raw.PrintOrderId;
					_context.Remove(raw);
					await _context.SaveChangesAsync();
					CheckPayments(printOrderId,
						pr => { pr.IsPaid = false; });
					await _context.SaveChangesAsync();
					transaction.Commit();
					return printOrderId;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, Const.ErrorMessages.CantSave);
					transaction.Rollback();
					throw;
				}
			}
		}

		private void CheckPayments(int printOrderId, Action<OptPrintOrder> action)
		{
			var raw = _context.OptPrintOrder
				.Include(printOrder => printOrder.PrintOrderPayments)
				.First(printOrder => printOrder.PrintOrderId == printOrderId);
			var sum = raw.PrintOrderPayments.Sum(payment => payment.Amount);
			if (sum < Convert.ToDecimal(raw.OrderSum))
			{
				action(raw);
			}
		}

		public async Task<int> AddPayment(PrintOrderPaymentModel model)
		{
			var raw = _mapper.Map<OptPrintOrderPayment>(model);
			await _context.AddAsync(raw);
			await _context.SaveChangesAsync();
			return raw.PrintOrderPaymentId;
		}

		public async Task<Dictionary<SemiproductModel, IEnumerable<PrintOrderSemiproductModel>>> GetPrintOrdersForSemiproductsAsync(IEnumerable<OptSemiproduct> rawSemiproducts, bool considerNotch)
		{
			var semiproducts = considerNotch ?
				rawSemiproducts.Where(smp => smp.SemiproductTypeId != (int)SemiProductType.Stickers)
				: rawSemiproducts;
			var semiprodsNameWithPrintOrders = new Dictionary<SemiproductModel, IEnumerable<PrintOrderSemiproductModel>>();
			var semiprodIds = semiproducts.Select(x => x.SemiproductId);

			var printOrdersBySemiproducts = await _context.OptPrintOrderSemiproduct
					.Include(printOrderSemiproduct => printOrderSemiproduct.PrintOrder)
					.Include(x => x.SemiProduct)
						.ThenInclude(x => x.SemiproductType)
					.Where(printSemiProduct =>
							semiprodIds.Contains(printSemiProduct.SemiproductId)
							&& !printSemiProduct.IsAssembled && printSemiProduct.PrintOrder.IsArchived == false)
					.OrderByDescending(x => x.PrintOrder.OrderDate)
				.ToListAsync();

			foreach (var semiprod in semiproducts)
			{
				var key = _mapper.Map<SemiproductModel>(semiprod);
				var values = _mapper.Map<IEnumerable<PrintOrderSemiproductModel>>(printOrdersBySemiproducts.Where(x => x.SemiproductId == semiprod.SemiproductId));
				semiprodsNameWithPrintOrders.Add(key, values);
			}

			return semiprodsNameWithPrintOrders;
		}

		public async Task<byte[]> GetArrearsReport()
		{
			var printOrdersForReport = await _context.OptPrintOrder
												.Where(x => x.IsPaid == false)
												.Include(x => x.PrintOrderPayments)
												.ToListAsync();
			var arrearsReport = new ArrearsReport(_configuration["Settings:Host"], printOrdersForReport);
			var report = arrearsReport.GenerateReport();
			return report;
		}

		public async Task<List<PrintOrderIncomingItemModel>> GetEmptyItems(int printOrderId)
		{
			var printOrder = await _context
				.OptPrintOrder
				.Include(x => x.PrintOrderIncomings)
					.ThenInclude(printOrderIncoming => printOrderIncoming.PrintOrderIncomingItems)
				.Include(printOrder => printOrder.PrintOrderSemiproducts)
					.ThenInclude(sProduct => sProduct.SemiProduct)
						.ThenInclude(semi => semi.Product.ProductSeries)
				.Include(printOrder => printOrder.PrintOrderSemiproducts)
					.ThenInclude(sProduct => sProduct.SemiProduct)
						.ThenInclude(semi => semi.Product.Country)
				.Include(printOrder => printOrder.PrintOrderSemiproducts)
					.ThenInclude(sProduct => sProduct.SemiProduct)
						.ThenInclude(semi => semi.SemiproductType)
			   .FirstAsync(sProduct => sProduct.PrintOrderId == printOrderId);
			var rawList = printOrder.PrintOrderSemiproducts;

			return rawList
				.Select(sProduct =>
				new PrintOrderIncomingItemModel
				{
					PrintOrderSemiproductName = _mapper.Map<SemiproductModel>(sProduct.SemiProduct).DisplayName,
					PrintOrderSemiproductId = sProduct.PrintOrderSemiproductId,
					PrintOrderSemiproductTypeId = sProduct.SemiProduct.SemiproductTypeId,
					ExistingAmount = GetExistingAmount(printOrder.PrintOrderIncomings, sProduct.PrintOrderSemiproductId)
				}
			).ToList();
		}

		private int GetExistingAmount(ICollection<OptPrintOrderIncoming> printOrderIncomings, int printOrderSemiproductId)
		{
			var amount = 0;
			foreach (var incoming in printOrderIncomings) {
				var incomingItem = incoming.PrintOrderIncomingItems.FirstOrDefault(item => item.PrintOrderSemiproductId == printOrderSemiproductId);
				if (incomingItem == null)
					continue;
				var multiplier = incoming.IncomingType == (int) PrintOrderIncomingType.Flaw ? -1 : 1;
				amount += multiplier * incomingItem.Amount;	
			}
			return	amount;
		}

		private async Task<string> CheckCanRemoveItems(IEnumerable<int> itemsToRemoveIds, int printOrderId)
		{
			var sb = new StringBuilder();
			var assembliesSemiprods = await _context.OptAssemblySemiproduct
									.Where(x => x.PrintOrderSemiproductId.HasValue && itemsToRemoveIds.Contains(x.PrintOrderSemiproductId.Value))
									.Include(x => x.Assembly)
									.ToListAsync();
			if (assembliesSemiprods != null && assembliesSemiprods.Any())
			{
				var assembliesDates = assembliesSemiprods.Select(x => x.Assembly.Date);
				sb.Append($"Невозможно удалить полуфабрикат из заказа печати, так как он участвует в сборках от: {string.Join(", ", assembliesDates)}");
			}

			var notchOrders = await _context.OptNotchOrderItem
									.Where(notchOrderItem => notchOrderItem.PrintOrderId == printOrderId)
									.Include(notchOrderItem => notchOrderItem.NotchOrder)
									.Select(notchOrderItem => notchOrderItem.NotchOrder)
									.ToListAsync();
			if (notchOrders != null && notchOrders.Any())
			{
				var notchOrderNumbers = notchOrders.Select(x => x.NotchOrderNumber).Distinct
					();
				sb.Append($"Заказ печати участвует в заказах надсечек №№: {string.Join(", ", notchOrderNumbers)}");
			}
			return (sb.Length > 0) ? sb.ToString() : string.Empty;
		}

		private async Task CleanDataAsync(OptPrintOrder printOrder)
		{
			try
			{
				if (printOrder == null || printOrder.PrintOrderId == 0)
					return;

				await _whJournalManager.RemoveByAction(printOrder);

				await _printOrderRepository.DeleteAsync(printOrder.PrintOrderId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"[Warehouse] Error while cleaning. Possible inconsistent data");
			}
		}
	}
}