using System.Collections.Generic;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class SemiproductPartDetails
	{
		public int SemiproductId { get; set; }
		public int SemiproductType { get; set; }
		public bool IsBack { get; set; }
		public int Inventory { get; set; }

		public int? SingleInventory { get; set; }

		public IEnumerable<WarehouseJournalModel> Journal { get; set; }
		public IEnumerable<WarehouseJournalModel> IncomeItems => Journal.Where(x => x.ActionTypeId == Enums.WhJournalActionType.Income);
		public int Income => IncomeItems.Sum(x => x.Amount);
		public IEnumerable<WarehouseJournalModel> OutcomeItems => Journal.Where(x => x.ActionTypeId == Enums.WhJournalActionType.Outcome);
		public int Outcome => OutcomeItems.Sum(x => x.Amount);
		public int CurrentAmount => (SingleInventory ??Inventory) + Income - Outcome;
	}
}
