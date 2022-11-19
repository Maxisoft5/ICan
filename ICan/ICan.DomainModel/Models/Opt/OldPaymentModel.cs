using System;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class OldPaymentModel
	{
		public Guid OrderId { get; set; }
		public DateTime OrderPaymentDate { get; set; }

		[Required]
		[Range(0.01, double.MaxValue, ErrorMessage = "Размер платежа должен быть положительным ")]
		public double Amount { get; set; }
	}
}
