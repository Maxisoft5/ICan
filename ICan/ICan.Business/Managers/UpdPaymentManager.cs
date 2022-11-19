using AutoMapper;
using ICan.Common.Domain;
using ICan.Common.Models.Opt;
using ICan.Common.Models.Opt.Report;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
	public class UpdPaymentManager : BaseManager
	{
		public UpdPaymentManager(ApplicationDbContext context, IMapper mapper, ILogger<ShopManager> logger)
			: base(mapper, context, logger)
		{
		}

		public async Task<IEnumerable<UpdPaymentModel>> GetCarriedAsync()
		{
			List<OptUpdPayment> payments = await GetUpdPaymentsRaw();
			var list = _mapper.Map<List<UpdPaymentModel>>(payments);
			var upds = GetUpdRaw();
			list.ForEach(payment => {
				var founded = upds.FirstOrDefault(upd => upd.ReportNum == payment.UpdNumber &&
					upd.ShopId == payment.ShopId && upd.ReportDate.Value.Year == payment.ReportDate.Year);
				if (founded == null)
				{
					payment.IsUnbound = true;
				}
			});
			
			return list.OrderBy(paym => paym.ShopId).ThenBy(upd => upd.Date);
		}

		public async Task<UpdCheckData> GetAsync()
		{
			IQueryable<OptReport> upds = GetUpdRaw();
			var updPayments = await _context
					.OptUpdPayment.ToListAsync();
			var list = new List<UpdPaymentCheckModel>();
			foreach (var upd in upds)
			{
				var payment = updPayments.FirstOrDefault(paym =>
				 paym.ShopId == upd.ShopId && paym.UpdNumber == upd.ReportNum && paym.ReportDate == upd.ReportDate.Value);
				if (payment != null)
					continue;
				// we should sum upds of one shop from the same days 
				var alreadyAddedItem = list.FirstOrDefault(added =>
				added.ShopId == upd.ShopId && added.ReportDate.Equals(upd.ReportDate.Value));
				if (alreadyAddedItem != null)
				{
					alreadyAddedItem.TotalSum += upd.TotalSum;
					alreadyAddedItem.ReportNums.Add(upd.ReportNum);
				}
				else
				{
					var item = new UpdPaymentCheckModel
					{
						ReportId = upd.ReportId,
						ReportNums = new List<string> { upd.ReportNum },
						ReportDate = upd.ReportDate.Value,
						ShopName = upd.Shop.Name,
						ShopId = upd.ShopId,
						TotalSum = upd.TotalSum,
						Postponement = upd.Shop.Postponement.Value,
					};

					list.Add(item);
				}
			}
			var groupped = list.ToLookup(item => item.ShopName, 
				item => item);
			var unbound = await GetUnboundCarriedAsync();
			return new UpdCheckData(groupped, unbound);
		}

		public void DeleteCarriedPayment(int id)
		{
			var payment = _context.OptUpdPayment.FirstOrDefault(payment => payment.UpdPaymentId == id);
			if (payment != null)
			{
				_context.Remove(payment);
				_context.SaveChanges();
			}
		}

		public void CarryPayment(IEnumerable<string> reportNums, int year)
		{
			var upds = _context.OptReport.Where(report =>
				reportNums.Contains(report.ReportNum) && report.ReportDate.HasValue &&
				report.ReportDate.Value.Year == year);

			OptReport upd;
			OptUpdPayment updPayment;
			var currentDate = System.DateTime.Now;
			foreach (var reportNum in reportNums)
			{
				upd = upds.First(item => item.ReportNum == reportNum);
				updPayment = new OptUpdPayment
				{
					UpdNumber = reportNum,
					ShopId = upd.ShopId,
					Date = currentDate,
					ReportDate = upd.ReportDate.Value
				};
				_context.Add(updPayment);
			}
			_context.SaveChanges();
		}

		private IQueryable<OptReport> GetUpdRaw()
		{
			var year = DateTime.UtcNow.Year;
			var upds = _context.OptReport
				.Include(report => report.Shop)
				.Where(report =>
				report.ReportDate.HasValue &&
				report.ReportDate.Value.Year == year &&
					report.ReportKindId == (int)ReportKind.UPD &&
					report.Shop.Postponement.HasValue &&
					report.Shop.Postponement > 0);
			return upds;
		}

		private async Task<List<OptUpdPayment>> GetUpdPaymentsRaw()
		{
			return await _context.OptUpdPayment.Include(paym => paym.Shop).ToListAsync();
		}

		private async Task<IEnumerable<UpdPaymentModel>> GetUnboundCarriedAsync()
		{
			var upds = GetUpdRaw();
			var payments = (await GetUpdPaymentsRaw()).ToList();
			var unbound = new List<OptUpdPayment>();
			payments.ForEach(payment => {
				var founded = upds.FirstOrDefault(upd => upd.ReportNum == payment.UpdNumber &&
					upd.ShopId == payment.ShopId && upd.ReportDate.Value.Year == payment.ReportDate.Year);
				if (founded == null)
				{
					unbound.Add(payment);
				}
			});
			var list = _mapper.Map<IEnumerable<UpdPaymentModel>>(unbound);
			return list.OrderBy(paym => paym.ShopId).ThenBy(upd => upd.Date);
		}
	}
}
