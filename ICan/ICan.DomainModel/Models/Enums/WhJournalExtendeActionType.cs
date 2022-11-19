using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Enums
{
	public enum WhJournalActionExtendedType
	{
		[Display(Name = "-")]
		None = 0,
		[Display(Name = "Заказ в оптовом модуле")]
		Order = 1,
		[Display(Name = "Приход на склад")]
		WarehouseArrival = 2,
		[Display(Name = "Сборка комплекта(приход)")]
		KitAssembly = 3,
		[Display(Name = "Благотворительность/маркетинг/брак")]
		Marketing = 4,
		[Display(Name = "Возврат")]
		Returning = 5,
		[Display(Name = "УПД")]
		UPD = 6,
		[Display(Name = "Часть сборки комплекта(расход)")]
		KitAssemblyPart = 7,
		[Display(Name = "Заказ печати")]
		PrintOrder = 8,
		[Display(Name = "Сборка на производстве")]
		AssemblyPart = 9,
		[Display(Name = "Заказ надсечки")]
		NotchOrder = 10,
		[Display(Name = "Приход в заказе надсечки")]
		NotchOrderIncoming = 11,
		[Display(Name = "Приход в заказе бумаги")]
		PaperOrderIncoming = 12,
		[Display(Name = "Перемещение бумаги")]
		MovingPaper = 13,
		[Display(Name = "Приход в заказе пружины")]
		SpringsIncoming = 14
	}
}
