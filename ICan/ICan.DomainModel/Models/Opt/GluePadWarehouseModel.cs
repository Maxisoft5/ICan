using ICan.Common.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class UniversalWarehouseModel
	{
		public IEnumerable<WarehouseModel> Warehouses { get; set; }
		public IEnumerable<WarehouseJournalModel> WhJournal { get; set; }


		public IEnumerable<WarehouseJournalModel> Outcomes => WhJournal.Where(x => x.ActionTypeId == WhJournalActionType.Outcome);
		public int OutcomeAmount => Outcomes != null ? Outcomes.Sum(x => x.Amount) : 0;

		public IEnumerable<WarehouseJournalModel> Incomings => WhJournal.Where(x => x.ActionTypeId == WhJournalActionType.Income);
		public int IncomingAmount => Incomings != null ? Incomings.Sum(x => x.Amount) : 0;


		public int? LastInventoryAmount { get; set; }
		public int Current => ( LastInventoryAmount ?? 0) + IncomingAmount - OutcomeAmount;

		public DateTime? LastInventoryDate { get; set; }
	}
}
