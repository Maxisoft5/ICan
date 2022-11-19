using ICan.Common.Models.Enums;
using ICan.Common.Utils;
using System;

namespace ICan.Common.Models.Opt
{
	public class WarehouseJournalModel
	{
		public long WarehousejournalId { get; set; }
		public DateTime ActionDate { get; set; }
		public WhJournalObjectType ObjectTypeId { get; set; }
		public long ObjectId { get; set; }
		public WarehouseType WarehouseTypeId { get; set; }
		public WhJournalActionType ActionTypeId { get; set; }
		public WhJournalActionExtendedType ActionExtendedTypeId { get; set; }
		public string ActionExtendedTypeName => ActionExtendedTypeId.GetDisplayName();
		public string ActionId { get; set; }
		public int Amount { get; set; }
		public string Comment { get; set; }
	}
}
