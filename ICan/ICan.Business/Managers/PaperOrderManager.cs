using AutoMapper;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ICan.Data.Context;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;

namespace ICan.Business.Managers
{
	public class PaperOrderManager : BaseManager
	{
		private readonly SemiproductManager _semiproductManager;
		private readonly WarehouseJournalManager _whJournalManager;
		private readonly IPaperOrderRepository _paperOrderRepository;
		private readonly IPrintOrderRepository _printOrderRepository;

		public PaperOrderManager(IMapper mapper, IPaperOrderRepository paperOrderRepository, ApplicationDbContext context, ILogger<BaseManager> logger, SemiproductManager semiproductManager,
			WarehouseJournalManager whJournalManager, IPrintOrderRepository printOrderRepository)
			: base(mapper, context, logger)
		{
			_semiproductManager = semiproductManager;
			_whJournalManager = whJournalManager;
			_paperOrderRepository = paperOrderRepository;
			_printOrderRepository = printOrderRepository;
		}

		public IEnumerable<PaperOrderModel> Get()
		{
			var list = _paperOrderRepository
						.Get();
			var modelList = _mapper.Map<IEnumerable<PaperOrderModel>>(list);
			return modelList;
		}

		public async Task Create(PaperOrderModel model)
		{
			var paymentDate = model.InvoiceDate;
			if (model.InvoiceDate.HasValue)
			{
				var supplierDelay = _context.OptCounterparty.Find(model.SupplierCounterPartyId).PaymentDelay ?? 0;
				paymentDate = paymentDate.Value.AddDays(supplierDelay);
			}

			var paperOrder = new OptPaperOrder
			{
				OrderDate = model.OrderDate,
				PaperId = model.PaperId,
				FormatId = model.FormatId,
				SheetCount = model.SheetCount,
				SheetPrice = model.OrderSum / model.SheetCount,
				InvoiceDate = model.InvoiceDate,
				PaymentDate = paymentDate,
				InvoiceNum = model.InvoiceNum,
				IsPaid = model.IsPaid,
				OrderSum = model.OrderSum,
				PaidSum = model.PaidSum,
				SupplierCounterPartyId = model.SupplierCounterPartyId,
				RecieverCounterPartyId = model.RecieverCounterPartyId,
				Comment = model.Comment,
				Weight = model.Weight
			};

			_context.Add(paperOrder);
			await _context.SaveChangesAsync();
		}

		public async Task<PaperOrderModel> GetAsync(int? id)
		{
			var raw = await _context.OptPaperOrder
				.Include(x => x.Paper)
				.Include(x => x.PrintOrderPapers)
					.ThenInclude(x => x.PrintOrder)
						.ThenInclude(x => x.PrintOrderSemiproducts)
				.Include(x => x.PaperOrderIncomings)
				.FirstOrDefaultAsync(m => m.PaperOrderId == id);

			var model = _mapper.Map<PaperOrderModel>(raw);
			var semiproductIds = model.PrintOrderPapers
				.Select(printOrderPaper => printOrderPaper.PrintOrder)
				.SelectMany(printOrder => printOrder.PrintOrderSemiproducts)
				.Select(printOrderSemi => printOrderSemi.SemiproductId)
				.ToList();

			var semiproductsRaw = _context.OptSemiproduct
				.Include(semiprod => semiprod.SemiproductType)
				.Include(semiprod => semiprod.Product)
					.ThenInclude(product => product.Country)
				.Where(semiprod => semiproductIds.Contains(semiprod.SemiproductId));

			var semiproducts = _mapper.Map<IEnumerable<SemiproductModel>>(semiproductsRaw);
			foreach (var printOrderPaper in model.PrintOrderPapers)
			{
				foreach (var printOrderSemiproduct in printOrderPaper.PrintOrder.PrintOrderSemiproducts)
				{
					printOrderSemiproduct.SemiProduct = semiproducts.First(semi => semi.SemiproductId == printOrderSemiproduct.SemiproductId);
				}
			}

			return model;
		}

		public async Task UpdateAsync(PaperOrderModel model)
		{
			var paymentDate = model.InvoiceDate;
			if (model.InvoiceDate.HasValue)
			{
				var supplierDelay = _context.OptCounterparty.Find(model.SupplierCounterPartyId).PaymentDelay ?? 0;
				paymentDate = paymentDate.Value.AddDays(supplierDelay);
			}
			var raw =
				_context.OptPaperOrder.Find(model.PaperOrderId);
			raw.OrderDate = model.OrderDate;
			raw.PaperId = model.PaperId;
			raw.FormatId = model.FormatId;
			raw.SheetCount = model.SheetCount;
			raw.SheetPrice = model.OrderSum / model.SheetCount;
			raw.OrderSum = model.OrderSum;
			raw.PaidSum = model.PaidSum;
			raw.InvoiceDate = model.InvoiceDate;
			raw.InvoiceNum = model.InvoiceNum;
			raw.IsPaid = model.IsPaid;
			raw.SupplierCounterPartyId = model.SupplierCounterPartyId;
			raw.RecieverCounterPartyId = model.RecieverCounterPartyId;
			raw.PaymentDate = paymentDate;
			raw.Comment = model.Comment;
			raw.Weight = model.Weight;

			_context.Update(raw);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var printOrdersWithPaperOrder = _printOrderRepository.Get().Where(x => x.PrintOrderPapers.Select(s => s.PaperOrder.PaperOrderId).Contains(id)).ToList();
			if (printOrdersWithPaperOrder != null)
				throw new UserException($"Заказ бумаги невозможно удалить, так как он выбран в следующих заказах печати: {string.Join(", ", printOrdersWithPaperOrder.Select(x => x.PrintingHouseOrderNum))}");
			var raw = await _context.OptPaperOrder.Include(x => x.PaperOrderIncomings).FirstOrDefaultAsync(m => m.PaperOrderId == id);
			var orderIncomings = raw.PaperOrderIncomings;
			await _whJournalManager.RemoveByAction(orderIncomings);
			_context.Remove(raw);
			await _context.SaveChangesAsync();
		}

		public IEnumerable<OptCounterparty> GetSuppliersAndRecievers()
		{
			return _context.OptCounterparty.ToList();
		}

		public async Task<int> GetAvailableAmount(int id)
		{
			var order = await GetAsync(id);
			return order.SheetCount - order.PaperOrderIncomings.Sum(incoming => incoming.Amount);
		}

		public async Task<IEnumerable<PaperOrderModel>> GetPaperOrdersForPrintOrderAsync(IEnumerable<long> existingList = null, int? semiProductId = null)
		{
			IQueryable<OptPaperOrder> rawList = _context.OptPaperOrder
			.Include(paperOrder => paperOrder.Paper)
			.Include(paperOrder => paperOrder.Format)
			.Include(paperOrder => paperOrder.PrintOrderPapers)
			.OrderBy(paperOrder => paperOrder.OrderDate);

			if (existingList != null && existingList.Any())
			{
				rawList = rawList.Where(paperOrder =>
					!existingList.Contains(paperOrder.PaperOrderId));
			}
			if (semiProductId.HasValue && semiProductId != 0)
			{
				var semiProduct = await _semiproductManager.GetSemiproductAsync(semiProductId.Value);
				var paperIds = semiProduct.SemiproductPapers.Select(sProd => sProd.PaperId).ToArray();
				rawList = rawList.Where(x => paperIds.Contains(x.PaperId));
			}

			var list = _mapper.Map<IEnumerable<PaperOrderModel>>(rawList);
			return list;
		}

		public async Task DeleteIncoming(int id)
		{
			var incoming = await _context.OptPaperOrderIncoming.FirstOrDefaultAsync(x => x.PaperOrderIncomingId == id);
			if(incoming != null)
			{
				_context.Remove(incoming);
				await _whJournalManager.RemoveByAction(id.ToString(), (int)WhJournalActionExtendedType.PaperOrderIncoming);
			}
		}

		public async Task AddIncoming(PaperOrderIncomingModel model)
		{
			if(model.Amount > 0)
			{
				using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
				{
					var incoming = _mapper.Map<OptPaperOrderIncoming>(model);
					await _context.OptPaperOrderIncoming.AddAsync(incoming);
					await _context.SaveChangesAsync();

					var paperOrder = _context.OptPaperOrder.First(x => x.PaperOrderId == model.PaperOrderId);

					var whJournal = new WarehouseJournalModel
					{
						ActionDate = incoming.IncomingDate,
						ActionId = incoming.PaperOrderIncomingId.ToString(),
						ActionTypeId = WhJournalActionType.Income,
						ActionExtendedTypeId = WhJournalActionExtendedType.PaperOrderIncoming,
						ObjectTypeId = WhJournalObjectType.Paper,
						ObjectId = paperOrder.PaperId,
						Amount = model.Amount,
						WarehouseTypeId = model.WarehouseTypeId,
					};
					await _whJournalManager.AddAsync(whJournal);
					await _context.SaveChangesAsync();
					await transaction.CommitAsync();
				}
			}
			else
			{
				throw new UserException("Количество прихода должно быть больше 0");
			}
		}
	}
}