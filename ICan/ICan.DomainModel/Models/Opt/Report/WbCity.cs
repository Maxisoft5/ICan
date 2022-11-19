using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt.Report
{
	public enum WbCity
	{
		[Display(Name = "-")]
		None = 0,
		[Display(Name = "Подольск")]
		Podolsk = 1,
		[Display(Name = "Новосибирск")]
		Novosibirsk = 2,
		[Display(Name = "Хабаровск")]
		Khabarovsk = 3,
		[Display(Name = "Краснодар")]
		Krasnodar = 4,
		[Display(Name = "Екатеринбург")]
		Ekaterinburg = 5,
		[Display(Name = "Санкт-Петербург")]
		SaintPetersburg = 6,
		[Display(Name = "Пушкино")]
		Pushkino = 10,
		[Display(Name = "Казань")]
		Kazan = 20,
		[Display(Name = "Домодедово")]
		Domodedovo = 22
	}
}
