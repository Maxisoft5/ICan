using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class NumberOfTurnsModel
	{
		public int NumberOfTurnsId { get; set; }
		[Display(Name = "Количество витков")]
		public int NumberOfTurns { get; set; }
		[Display(Name = "Производитель")]
		public string Manufacturer { get; set; }
		public IEnumerable<SpringModel> Springs { get; set; }
	}
}
