using System;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class GluePadIncomingModel
	{
		public int Id { get; set; }
		[Display(Name = "Комментарий")]
		public string Description { get; set; }
		[Display(Name = "Сумма")]
		public double Price { get; set; }
		[Display(Name = "Кол-во")]
		public int Amount { get; set; }
		public DateTime IncomingDate { get; set; } = DateTime.Now;
	}
}
