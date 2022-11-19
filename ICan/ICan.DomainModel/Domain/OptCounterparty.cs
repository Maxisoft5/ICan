using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public class OptCounterparty
	{
		public OptCounterparty()
		{
			PaperOrderSuppliers = new HashSet<OptPaperOrder>();
			PaperOrderRecievers = new HashSet<OptPaperOrder>();
			WarehouseTypes = new HashSet<OptWarehouseType>();
		}

		public int CounterpartyId { get; set; }

		public string Name { get; set; }

		public string Consignee { get; set; }
		public string Inn { get; set; }

		public bool Enabled { get; set; }

		public int PaperOrderRoleId { get; set; }
		public int? PaymentDelay { get; set; }

		public OptPaperOrderRole PaperOrderRole { get; set; }
		public ICollection<OptPaperOrder> PaperOrderSuppliers { get; set; }
		public ICollection<OptPaperOrder> PaperOrderRecievers { get; set; }
		public ICollection<OptWarehouseType> WarehouseTypes { get; set; }
	}
}
