using System;

namespace ICan.Common.Domain
{
	public partial class OptWarehouseJournal
	{
		public long WarehousejournalId { get; set; }
		public DateTime ActionDate { get; set; }
		public int ObjectTypeId { get; set; }
		public int ObjectId { get; set; }
		public int ActionTypeId { get; set; }
		public int ActionExtendedTypeId { get; set; }
		public int WarehouseTypeId { get; set; }
		public string ActionId { get; set; }
		public int Amount { get; set; }
		public string Comment { get; set; }
		public OptWarehouseType WarehouseType { get; set; }
	}
}
