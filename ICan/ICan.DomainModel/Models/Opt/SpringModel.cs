using ICan.Common.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class SpringModel
	{
		public int SpringId { get; set; }
		[Display(Name = "Название пружины")]
		public string SpringName { get; set; }
		[Display(Name = "Толщина блока, мм")]
		public int BlockThickness { get; set; }
		[Display(Name = "Шаг")]
		public StepEnum Step { get; set; }
		public int NumberOfTurnsId { get; set; }
		[Display(Name = "Количество витков")]
		public NumberOfTurnsModel NumberOfTurns { get; set; }
	}
}
