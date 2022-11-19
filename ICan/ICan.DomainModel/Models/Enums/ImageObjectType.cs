using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Enums
{
	public enum ImageObjectType
	{
		[Display(Name = "Неизвестно")]
		None = 0,
		[Display(Name = "Тетрадь")]
		Notebook = 1,
		[Display(Name = "Материал")]
		Material = 2,
		[Display(Name = "Рассылка")]
		Campaign = 3,
	}
}