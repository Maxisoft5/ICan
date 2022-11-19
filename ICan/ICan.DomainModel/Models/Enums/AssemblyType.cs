using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Enums
{
	public enum AssemblyType
	{
		[Display(Name = "-")]
		None = 0,
		[Display(Name = "Сборка")]
		Assembly = 1,
		[Display(Name = "Навивка")]
		Wind = 2
	}
}
