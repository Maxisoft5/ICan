using ICan.Common.Domain;
using System;
using System.Collections.Generic;

namespace ICan.Common.Repositories
{
	public interface IWbReportRepository
	{
		IEnumerable<string> GetWarehouses();
		IEnumerable<T> GetItems<T>(DateTime startDate, DateTime endDate) where T : class, IWBItemWithProductId;
	}
}