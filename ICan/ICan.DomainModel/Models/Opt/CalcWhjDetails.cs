using ICan.Common.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class CalcWhjDetails
	{
		public int ProductId { get; set; }

		[Display(Name = "Название")]
		public string Name { get; set; }

		[Display(Name = "Комплект")]
		public bool IsKit { get; set; }
		public bool AssemblesAsKit { get; set; }

		public int? ProductSeriesId { get; set; }
		public string ProductKindDisplayName =>
			IsKit ? "комплекта" : (Const.CalendarSeriedId == ProductSeriesId ? "календаря" : "тетради");

		[Display(Name = "Инвентаризация")]
		public int Inventory { get; set; }
		public DateTime? InventoryDate { get; set; }

		public IEnumerable<WarehouseJournalModel> Journal { get; set; }


		[Display(Name = "Приход")]
		public IEnumerable<WarehouseJournalModel> ArrivedItems => Journal.Where(whj => whj.ActionExtendedTypeId == WhJournalActionExtendedType.WarehouseArrival)
				.OrderBy(item => item.ActionDate);

		[Display(Name = "Приход")]
		public int Arrived => ArrivedItems.Sum(item => item.Amount);


		[Display(Name = "Участвует в сборках комплектов (только для обычной тетради, не комплекта)")]
		public IEnumerable<WarehouseJournalModel> KitAssemblyPartItems => Journal.Where(whj => whj.ActionExtendedTypeId == WhJournalActionExtendedType.KitAssemblyPart)
				.OrderBy(item => item.ActionDate);

		[Display(Name = "Участвует в сборках комплектов (только для обычной тетради, не комплекта)")]
		public int KitAssemblyPart => KitAssemblyPartItems.Sum(item => item.Amount);


		[Display(Name = "Сборка комплектов (только для комплекта)")]
		public IEnumerable<WarehouseJournalModel> KitAssemblyItems => Journal.Where(whj => whj.ActionExtendedTypeId == WhJournalActionExtendedType.KitAssembly)
				.OrderBy(item => item.ActionDate);

		[Display(Name = "Сборка комплектов (только для комплекта)")]
		public int KitAssembly => KitAssemblyItems.Sum(item => item.Amount);


		[Display(Name = "Отгрузка по УПД")]
		public IEnumerable<WarehouseJournalModel> UPDItems => Journal.Where(whj => whj.ActionExtendedTypeId == WhJournalActionExtendedType.UPD)
			.OrderBy(item => item.ActionDate);

		[Display(Name = "Отгрузка по УПД")]
		public int UPD => UPDItems.Sum(item => item.Amount);


		[Display(Name = "Маркетинг")]
		public IEnumerable<WarehouseJournalModel> MarketingItems => Journal.Where(whj => whj.ActionExtendedTypeId == WhJournalActionExtendedType.Marketing)
				.OrderBy(item => item.ActionDate);

		[Display(Name = "Маркетинг")]
		public int Marketing => MarketingItems.Sum(item => item.Amount);

		[Display(Name = "Возврат")]
		public IEnumerable<WarehouseJournalModel> ReturnItems => Journal.Where(whj => whj.ActionExtendedTypeId == WhJournalActionExtendedType.Returning)
				.OrderBy(item => item.ActionDate);

		[Display(Name = "Возврат")]
		public int Return => ReturnItems.Sum(item => item.Amount);


		[Display(Name = "Отгрузка через оптовый модуль")]
		public IEnumerable<WarehouseJournalModel> OrderProductItems => Journal.Where(whj => whj.ActionExtendedTypeId == WhJournalActionExtendedType.Order).OrderBy(ord => ord.Comment);

		[Display(Name = "Отгрузка через оптовый модуль")]
		public int OrderProduct => OrderProductItems.Sum(item => item.Amount);


		[Display(Name = "Результат")]
		public int Current =>
				 (SingleInventory ?? Inventory) + Arrived - UPD - Marketing + Return - OrderProduct - KitAssemblyPart + KitAssembly;

		[Display(Name = "Дата единичной инвентаризации")]
		public DateTime? SingleInventoryDate { get; set; }
		[Display(Name = "Единичная инвентаризация")]
		public int? SingleInventory { get; set; }
	}
}