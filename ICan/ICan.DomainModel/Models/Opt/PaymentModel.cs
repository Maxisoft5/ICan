using ICan.Common.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class PaymentModel
	{
		public int PaymentId { get; set; }
		[Display(Name = "Дата платежа")]
		public DateTime PaymentDate { get; set; }
		[Display(Name = "Сумма")]
		[Required]
		[Range(0.01, double.MaxValue, ErrorMessage = "Размер платежа должен быть положительным ")]
		public decimal Amount { get; set; }
		public PaymentType PaymentType { get; set; }
		public int OrderId { get; set; }

		public string DisplayDate => PaymentDate.ToString("dd.MM.yyyy");
		public string DisplayAmount => Amount.ToString("N2");
	}
}
