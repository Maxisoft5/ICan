using System;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class SpringOrderIncomingModel
	{
		public int SpringOrderIncomingId { get; set; }
		[Required]
		[Range(1, int.MaxValue, ErrorMessage = Const.ValidationMessages.PositiveFieldMessage)]
		[Display(Name="Количество бобин")]
		public int SpoolCount { get; set; }
		[Display(Name = "Общее количество витков")]
		public int NumberOfTurnsCount { get; set; }
		[Required]
		[Display(Name = "Дата прихода")]
		public DateTime IncomingDate { get; set; }
		public int SpringOrderId { get; set; }
		public SpringOrderModel SpringOrder { get; set; }

		public int SpringNumerOfTurns { get; set; }
	}
}
