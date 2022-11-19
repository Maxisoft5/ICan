using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Enums
{
	public enum PrintOrderIncomingType
	{
		[Display(Name = "-")]
		None = 0,
		[Display(Name = "Обычный")]
		Ordinal = 1,
		[Display(Name = "Сверхтираж")]
		OverPrint = 2,
		[Display(Name = "Брак")]
		Flaw = 3
	}
}
