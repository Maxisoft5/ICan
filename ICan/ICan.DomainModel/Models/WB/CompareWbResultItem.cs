using System;

namespace ICan.Common.Models.WB
{
	public class CompareWbResultItem
	{
		public DateTime Date { get; set; }
		public int? OrdersFromApi { get; set; }
		public int? OrdersFromFile { get; set; }
		public int? OrdersDiff => OrdersFromFile - OrdersFromApi;
		public int? SalesFromApi { get; set; }
		public int? SalesFromFile { get; set; }
		public int? SalesDiff => SalesFromFile - SalesFromApi;
	}
}
