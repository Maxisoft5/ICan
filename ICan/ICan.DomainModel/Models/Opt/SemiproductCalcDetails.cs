using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class SemiproductCalcDetails
	{
		[Display(Name = "Инвентаризация")]
		public int Inventory { get; set; }

		[Display(Name = "Дата инвентаризации")]
		public DateTime? InventoryDate { get; set; }

		[Display(Name = "В процессе печати")]
		public int PrintingInProcess { get; set; }

		[Display(Name = "Приход в заказах печати ПОСЛЕ инвентаризации")]
		public int PrintOrderSum => PrintOrderIncomingItems != null ?
			PrintOrderIncomingItems.Where(x => x.ActionTypeId == (int)WhJournalActionType.Income).Sum(x => x.Amount)
			- PrintOrderIncomingItems.Where(x => x.ActionTypeId == (int)WhJournalActionType.Outcome).Sum(x => x.Amount)
			: 0;

		[Display(Name = "Приход в заказах печати ПОСЛЕ инвентаризации")]
		public IEnumerable<OptWarehouseJournal> PrintOrderIncomingItems { get; set; }


		[Display(Name = "Сборка тетрадей")]
		public int AssemblySum => Assemblies != null ? Assemblies.Sum(x => x.Amount) : 0;

		[Display(Name = "Сборка тетрадей")]
		public IEnumerable<OptAssembly> Assemblies { get; set; }

		public int ProductId { get; set; }

		public int SemiproductId { get; set; }

		public int SemiproductTypeId { get; set; }

		[Display(Name = "Название")]
		public string SemiproductName { get; set; }

		[Display(Name = "Результат")]
		public int Current =>
				(SingleInventory ?? Inventory) +
			SemiproductsReadyJournalSum;
		//	PrintOrderSum - AssemblySum - KitAssemblySum;

		public IEnumerable<OptPrintOrder> PrintOrders { get; set; }

		[Display(Name = "Сборка комплекта")]
		public IEnumerable<WarehouseItemModel> KitAssemblies { get; set; }
		[Display(Name = "Сборка комплекта")]
		public int KitAssemblySum => KitAssemblies != null ? KitAssemblies.Sum(x => x.Amount) : 0;
		public IEnumerable<PrintOrderRestAmountModel> PrintOrdersInProgress { get; set; }
		[Display(Name = "Название")]
		public string SemiproductDisplayName { get; set; }

		[Display(Name = "Дата единичной инвентаризации")]
		public DateTime? SingleInventoryDate { get; set; }
		[Display(Name = "Единичная инвентаризация")]
		public int? SingleInventory { get; set; }


		[Display(Name = "Не надсечённые наклейки")]
		public int StickersUnNotchedSum { get; set; }


		[Display(Name = "Наклейки на надсечке в Аксае")]
		public int StickersNotchingSum { get; set; }

		public int SemiproductsReadyJournalSum { get; set; }
	}
}