using System;
using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public partial class OptPaperOrder
	{
		public OptPaperOrder()
		{
			PrintOrderPapers = new HashSet<OptPrintOrderPaper>();
			PaperOrderIncomings = new HashSet<OptPaperOrderIncoming>();
		}

		public int PaperOrderId { get; set; }

		public DateTime OrderDate { get; set; }

		public int PaperId { get; set; }

		public int FormatId { get; set; }

		public int SheetCount { get; set; }

		public double Weight { get; set; }

		public double SheetPrice { get; set; }

		public double OrderSum { get; set; }

		public double? PaidSum { get; set; }

		public string InvoiceNum { get; set; }

		public DateTime? InvoiceDate { get; set; }

		public DateTime? PaymentDate { get; set; }

		public bool IsPaid { get; set; }

		public string Comment { get; set; }

		public int SupplierCounterPartyId { get; set; }

		public int? RecieverCounterPartyId { get; set; }

		public OptCounterparty SupplierCounterParty { get; set; }

		public OptCounterparty RecieverCounterParty { get; set; }

		public OptFormat Format { get; set; }

		public OptPaper Paper { get; set; }

		public ICollection<OptPrintOrderPaper> PrintOrderPapers { get; set; }
		public ICollection<OptPaperOrderIncoming> PaperOrderIncomings { get; set; }
	}
}
