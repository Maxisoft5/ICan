using System;

namespace ICan.Common.Domain
{
	public class OptUpdPayment
	{
		public int UpdPaymentId { get; set; }

		public int ShopId { get; set; }

		public DateTime Date { get; set; }
		
		public DateTime ReportDate { get; set; }

		public string UpdNumber { get; set; }

		public string Comment { get; set; }

		public OptShop Shop { get; set; }
	}
}
