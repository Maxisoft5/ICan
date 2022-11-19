using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Enums
{
	public enum WhJournalObjectType
	{
		[Display(Name = "Неизвестно")]
		None = 0,
		[Display(Name = "Тетрадь")]
		Notebook = 1,
		[Display(Name = "Полуфабрикат")]
		Semiproduct = 2,
		[Display(Name = "Бумага")]
		Paper = 3,
		[Display(Name = "Клеевая подушка")]
		GluePad = 4,
		[Display(Name = "Пружина")]
		Spring = 5
	}
}