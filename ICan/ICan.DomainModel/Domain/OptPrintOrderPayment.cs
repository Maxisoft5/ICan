using System;

namespace ICan.Common.Domain
{
	public class OptPrintOrderPayment
	{
		public int PrintOrderPaymentId { get; set; }
		
		public int PrintOrderId { get; set; }
		
		public DateTime Date { get; set; }

		public decimal Amount { get; set; }

		public OptPrintOrder PrintOrder { get; set; }
	}
}
