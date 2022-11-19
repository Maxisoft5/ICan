using System;
using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public partial class OptPrintOrder
	{
		public OptPrintOrder()
		{
			PrintOrderSemiproducts = new HashSet<OptPrintOrderSemiproduct>();
			PrintOrderIncomings = new HashSet<OptPrintOrderIncoming>();
			PrintOrderPapers = new HashSet<OptPrintOrderPaper>();
			PrintOrderPayments = new HashSet<OptPrintOrderPayment>();
		}
		public int PrintOrderId { get; set; }
		public string PrintingHouseOrderNum { get; set; }
		public DateTime OrderDate { get; set; }
		public int Printing { get; set; }
		public DateTime? PaymentDate { get; set; }
		public double OrderSum { get; set; }
		public bool? IsPaid { get; set; }
		public bool? IsAssembled { get; set; }
		public bool? ConfirmPrint { get; set; }
		public string CheckNumber { get; set; }
		public string Comment { get; set; }
		public int? PaperPlannedExpense { get; set; }
		public bool IsArchived { get; set; }

		public OptNotchOrderItem NotchOrderItem { get; set; }
		public ICollection<OptPrintOrderSemiproduct> PrintOrderSemiproducts { get; set; }
		public ICollection<OptPrintOrderIncoming> PrintOrderIncomings { get; set; }
		public ICollection<OptPrintOrderPaper> PrintOrderPapers { get; set; }
		public ICollection<OptPrintOrderPayment> PrintOrderPayments { get; set; }
	}
}
