using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Enums
{
	public enum WarehouseType
	{
		[Display(Name = "Неизвестно")]
		None = 0,
		[Display(Name = "Склад готовых тетрадей")]
		NotebookReady = 1,
		[Display(Name = "Склад готовых полуфабрикатов")]
		SemiproductReady = 2,
		[Display(Name = "Склад готовой бумаги в Я Могу")]
		PaperReady = 3,
		[Display(Name = "Склад готовой бумаги в Зетапринт")]
		PaperReadyZetaPrint = 5,
		[Display(Name = "Не надсечённые наклейки на складе в Я Могу")]
		StickersUnNotched = 6,
		[Display(Name = "Наклейки на надсечке в Аксае")]
		StickersNotching = 7,
		[Display(Name = "Склад клеевых подушек")]
		GluePads = 8,
		[Display(Name = "Склад пружин")]
		Spings = 9,
	}
}
