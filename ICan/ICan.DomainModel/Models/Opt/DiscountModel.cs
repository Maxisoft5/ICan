using System;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class DiscountModel
	{
		public long DiscountId { get; set; }

		[Display(Name = "Значение")]
		[Range(0, 100, ErrorMessage = Const.ValidationMessages.MinMaxRangeExceeded)]
		[Required(ErrorMessage = Const.ValidationMessages.RequiredFieldMessage)]
		public double Value { get; set; }

		[Display(Name = "Доступна")]
		public bool Enabled { get; set; } = true;
		public bool IsArchived { get; set; }
		[Display(Name = "Описание")]
		[StringLength(450)]
		public string Description { get; set; }

		public DateTime CreateDate { get; set; }

	}
}
