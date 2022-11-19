using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public class OptWarehouseType
	{
		public OptWarehouseType()
		{
			Warehouses = new HashSet<OptWarehouse>();
			WarehouseJournal = new HashSet<OptWarehouseJournal>();
			PaperOrderIncomings = new HashSet<OptPaperOrderIncoming>();
		}

		public int WarehouseTypeId { get; set; }

		public string Name { get; set; }

		public string Comment { get; set; }

		public bool ReadyToUse { get; set; }

		public int WarehouseObjectType { get; set; }

		public int? CounterpartyId { get; set; }

		public OptCounterparty Counterparty { get; set; }

		public ICollection<OptWarehouse> Warehouses { get; set; }

		public ICollection<OptWarehouseJournal> WarehouseJournal { get; set; }
		public ICollection<OptPaperOrderIncoming> PaperOrderIncomings { get; set; }
	}
}
