using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Enums
{
	public enum StepEnum
	{
		[Display(Name = "5/16")]
		FiveSixteenth = 1,
		[Display(Name = "3/8")]
		ThreeEights = 2,
		[Display(Name = "7/16")]
		SevenSixteenth = 3
	}
}
