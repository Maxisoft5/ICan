using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Enums
{
	public enum OrderStatus
	{
        [Display(Name = "Неизвестно")]
        Unknown,
		[Display(Name = "Новый")]
		New,
		[Display(Name = "Сборка")]
		Assembling,
		[Display(Name = "Выполнен")]
		Done
	}
}