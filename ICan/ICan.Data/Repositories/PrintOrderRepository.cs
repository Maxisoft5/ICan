using ICan.Common.Domain;
using ICan.Common.Models.Opt;
using ICan.Common.Repositories;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Data.Repositories
{
	public class PrintOrderRepository : BaseRepository, IPrintOrderRepository
	{
		public PrintOrderRepository(ApplicationDbContext context) : base(context)
		{		
		}

		public async Task<OptPrintOrder> CreateAsync(PrintOrderModel model)
		{
			var raw = new OptPrintOrder
			{
				OrderDate = model.OrderDate,
				PrintingHouseOrderNum = model.PrintingHouseOrderNum,
				Printing = model.Printing,
				OrderSum = model.OrderSum,
				PaymentDate = model.PaymentDate,
				IsPaid = model.IsPaid,
				IsAssembled = model.IsAssembled,
				ConfirmPrint = model.ConfirmPrint,
				CheckNumber = model.CheckNumber,
				Comment = model.Comment,
				PaperPlannedExpense = model.PaperPlannedExpense
			};

			await _context.AddAsync(raw);

			foreach (var semiproduct in model.PrintOrderSemiproducts)
			{
				var printOrderSemiproduct = new OptPrintOrderSemiproduct
				{
					PrintOrder = raw,
					SemiproductId = semiproduct.SemiproductId
				};

				await _context.AddAsync(printOrderSemiproduct);
			}

			foreach (var item in model.PrintOrderPapers)
			{
				var printOrderPaper = new OptPrintOrderPaper
				{
					PrintOrder = raw,
					PaperOrderId = item.PaperOrderId,
					SheetsTakenAmount = item.SheetsTakenAmount,
					IsSent = item.IsSent,
				};

				await _context.AddAsync(printOrderPaper);
			}

			await _context.SaveChangesAsync();
			return raw;
		}

		public async Task DeleteAsync(int printOrderId)
		{
			var raw = await _context.OptPrintOrder.FirstOrDefaultAsync(printOrder => printOrder.PrintOrderId == printOrderId);
			if (raw == null)
				return;

			_context.Remove(raw);
			await _context.SaveChangesAsync();
		}

		public async Task<OptPrintOrder> GetByIdAsync(int id)
		{
			var raw = await _context.OptPrintOrder
		   .Include(printOrder => printOrder.PrintOrderSemiproducts)
			  .ThenInclude(pOrder => pOrder.SemiProduct)
			  .ThenInclude(pOrder => pOrder.BlockType)
		   .Include(printOrder => printOrder.PrintOrderPayments)
		   .Include(printOrder => printOrder.PrintOrderIncomings)
			   .ThenInclude(printOrderIncoming => printOrderIncoming.PrintOrderIncomingItems)
		   .Include(printOrder => printOrder.PrintOrderPapers)
			   .ThenInclude(printOrderPaper => printOrderPaper.PaperOrder)
				   .ThenInclude(printOrderPaper => printOrderPaper.Paper)
		   .Include(printOrder => printOrder.PrintOrderPapers)
			   .ThenInclude(printOrderPaper => printOrderPaper.PaperOrder)
			   .ThenInclude(printOrderPaper => printOrderPaper.PrintOrderPapers)
		   .FirstOrDefaultAsync(m => m.PrintOrderId == id);
			return raw;
		}


		public IEnumerable<OptPrintOrder> Get()
		{
			var rawList = _context.OptPrintOrder
				.Include(printOrder => printOrder.PrintOrderPapers)
					.ThenInclude(s => s.PaperOrder).ThenInclude(t => t.Paper)
				.Include(printOrder => printOrder.PrintOrderPayments)
				.Include(printOrder => printOrder.PrintOrderIncomings)
					.ThenInclude(printOrderIncoming => printOrderIncoming.PrintOrderIncomingItems)
				.Include(printOrder => printOrder.PrintOrderSemiproducts)
					.ThenInclude(sProduct => sProduct.SemiProduct)
					.ThenInclude(s => s.Product)
					.ThenInclude(s => s.ProductSeries)
				.Include(printOrder => printOrder.PrintOrderSemiproducts)
					.ThenInclude(sProduct => sProduct.SemiProduct)
					.ThenInclude(s => s.Product)
					.ThenInclude(s => s.Country)
				.Include(printOrder => printOrder.PrintOrderSemiproducts)
					.ThenInclude(sProduct => sProduct.SemiProduct)
					.ThenInclude(s => s.SemiproductType)
				.Include(printOrder => printOrder.PrintOrderSemiproducts)
					.ThenInclude(sProduct => sProduct.SemiProduct)
					.ThenInclude(sProdict => sProdict.BlockType)
				.Include(printOrder => printOrder.PrintOrderPapers)
					.ThenInclude(paper => paper.PaperOrder)
				.OrderBy(printOrder => printOrder.PrintOrderSemiproducts.OrderBy(sProd => sProd.SemiProduct.SemiproductTypeId).First().SemiProduct.SemiproductTypeId)
				.ThenByDescending(printOrder => printOrder.OrderDate)
				.AsNoTracking()
				.ToList();
			return rawList;
		}

		public List<int> GetSemiproductdById(int id)
		{
			var semiproductIds = _context.OptPrintOrderSemiproduct
			   .Where(printOrderS => printOrderS.PrintOrderId == id)
			   .Select(printOrderS => printOrderS.SemiproductId)
			   .ToList();
			return semiproductIds;
		}

        public async Task<IEnumerable<OptPrintOrder>> GetByPaperIdAsync(int paperId)
        {
			return await _context.OptPrintOrderPaper
				.Where(x => x.PaperOrder.Paper.PaperId == paperId
				&& x.PrintOrder.IsArchived == false).Select(x => x.PrintOrder).AsNoTracking().ToListAsync();

        }

    }
}
