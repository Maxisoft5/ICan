using ICan.Common.Domain;
using ICan.Common.Repositories;
using ICan.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ICan.Data.Repositories
{
	public class WbReportRepository : BaseRepository, IWbReportRepository
	{
		public WbReportRepository(ApplicationDbContext context) : base(context)
		{
		}

		public IEnumerable<T> GetItems<T>(DateTime startDate, DateTime endDate) where T: class, IWBItemWithProductId
		{
			var list = _context.Set<T>().Where(wbSale => wbSale.Date.Date>= startDate.Date && wbSale.Date.Date <= endDate.Date).ToList();
			 
			return list;
		}

		public IEnumerable<string> GetWarehouses()
		{
			var list = _context.OptWbSale.Select(wbSale => wbSale.WarehouseName).Distinct().ToList();
			list.AddRange(_context.OptWbOrder.Select(wbSale => wbSale.WarehouseName).Distinct());
			list = list.Distinct().ToList();
			return list;
		}
	}
}
