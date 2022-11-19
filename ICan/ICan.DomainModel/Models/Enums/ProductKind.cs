using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Enums
{
	public enum ProductKind
	{
		[Display(Name = "Неизвестно")]
		None = 0,
		[Display(Name = "Тетрадь")]
		Notebook = 1,
		[Display(Name = "Кое-что ещё")]
		SmthElse = 2,
		[Display(Name = "Клеевая подушка")]
		GluePad= 3
	}
}
