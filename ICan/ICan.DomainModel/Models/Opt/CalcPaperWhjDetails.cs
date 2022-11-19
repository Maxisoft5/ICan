using ICan.Common.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace ICan.Common.Models.Opt
{
	public class CalcPaperWhjDetails
	{
		public int PaperId { get; set; }

		[Display(Name = "Название")]
		public string PaperName { get; set; }

		public WarehouseType WarehouseType { get; set; }

		public IEnumerable<WarehouseJournalModel> Journal { get; set; }

		[Display(Name = "Инвентаризация")]
		public int InventoryAmount { get; set; }
		[Display(Name = "Дата инвентаризации")]
		public DateTime? InventoryDate { get; set; }

		[Display(Name = "Единичная инвентаризация")]
		public int? SingleInventoryAmount { get; set; }

		[Display(Name = "Дата единичной инвентаризации")]
		public DateTime? SingleInventoryDate { get; set; }

		[Display(Name = "Приход в заказах бумаги ПОСЛЕ инвентаризации")]
		public int PaperOrderSum => PaperOrderIncomingItems != null ? PaperOrderIncomingItems.Sum(x => x.Amount) : 0;


		[Display(Name = "Приход в заказах бумаги ПОСЛЕ инвентаризации")]
		public IEnumerable<WarehouseJournalModel> PaperOrderIncomingItems => Journal != null && Journal.Any() ?
			Journal.Where(x => x.ActionExtendedTypeId == WhJournalActionExtendedType.PaperOrderIncoming)
			.OrderBy(paperOrderInc => paperOrderInc.ActionDate)
			: Enumerable.Empty<WarehouseJournalModel>();

		[Display(Name = "Расход в заказах печати ПОСЛЕ инвентаризации")]
		public int PrintOrderPaperSum => PrintOrderPapers != null ? PrintOrderPapers.Sum(x => x.Amount) : 0;


		[Display(Name = "Расход в заказах печати ПОСЛЕ инвентаризации")]
		public IEnumerable<WarehouseJournalModel> PrintOrderPapers => Journal != null && Journal.Any()
			? Journal.Where(x => x.ActionExtendedTypeId == WhJournalActionExtendedType.PrintOrder)
				.OrderBy(journal => journal.ActionDate)
		: Enumerable.Empty<WarehouseJournalModel>();

		[Display(Name = "Перемещение бумаги")]
		public IEnumerable<WarehouseJournalModel> MovingPaper => Journal != null && Journal.Any()
			? Journal.Where(x => x.ActionExtendedTypeId == WhJournalActionExtendedType.MovingPaper)
				.OrderBy(journal => journal.ActionDate)
			: Enumerable.Empty<WarehouseJournalModel>();

		[Display(Name = "Перемещение бумаги (получение)")]
		public int MovingPaperIncomeSum => MovingPaper != null ?
			MovingPaper.Where(x => x.ActionTypeId == WhJournalActionType.Income).Sum(x => x.Amount) : 0;

		[Display(Name = "Перемещение бумаги (отправка)")]
		public int MovingPaperOutcomeSum => MovingPaper != null ?
			MovingPaper.Where(x => x.ActionTypeId == WhJournalActionType.Outcome).Sum(x => x.Amount) : 0;

		[Display(Name = "Результат")]
		public int Current => (SingleInventoryAmount ?? InventoryAmount) + PaperOrderSum + MovingPaperIncomeSum - PrintOrderPaperSum - MovingPaperOutcomeSum;

		public IEnumerable<PaperOrderIncomingModel> PaperOrderIncomings { get; set; }
		public IEnumerable<PrintOrderPaperModel> PrintOrderPaperExtended { get; set; }
	}
}
