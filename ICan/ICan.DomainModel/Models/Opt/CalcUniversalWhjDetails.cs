using ICan.Common.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class CalcUniversalWhjDetails
	{
		public int ObjectId { get; set; }

		[Display(Name = "Название")]
		public string ObjectDisplayName { get; set; }

		//[Display(Name = "Склад")]
		public WarehouseType WarehouseType { get; set; }

		public IEnumerable<WarehouseJournalModel> Journal { get; set; }

		[Display(Name = "Инвентаризация")]
		public int InventoryAmount { get; set; }
		[Display(Name = "Дата инвентаризации")]
		public DateTime? InventoryDate { get; set; }


		[Display(Name = "Приход ПОСЛЕ инвентаризации")]
		public int IncomeSum => IncomingItems != null ? IncomingItems.Sum(x => x.Amount) : 0;


		[Display(Name = "Приход ПОСЛЕ инвентаризации")]
		public IEnumerable<WarehouseJournalModel> IncomingItems => Journal != null && Journal.Any() ?
			Journal.Where(x => x.ActionTypeId == WhJournalActionType.Income).OrderBy(action=> action.ActionDate) : null;

		[Display(Name = "Расход ПОСЛЕ инвентаризации")]
		public int OutcomeSum => OutcomeItems != null ? OutcomeItems.Sum(x => x.Amount) : 0;


		[Display(Name = "Расход ПОСЛЕ инвентаризации")]
		public IEnumerable<WarehouseJournalModel> OutcomeItems => Journal != null && Journal.Any() ?
			Journal.Where(x => x.ActionTypeId == WhJournalActionType.Outcome).OrderBy(action => action.ActionDate) : null;

		[Display(Name = "Результат")]
		public int Current => InventoryAmount + IncomeSum - OutcomeSum;
	}
}
