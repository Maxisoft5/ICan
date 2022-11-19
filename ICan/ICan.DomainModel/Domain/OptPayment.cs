using ICan.Common.Models.Enums;
using System;

namespace ICan.Common.Domain
{
	public class OptPayment
	{
		public int PaymentId { get; set; }
		public DateTime PaymentDate { get; set; }
		public decimal Amount { get; set; }
		public PaymentType PaymentType { get; set; }
		public int OrderId { get; set; }
	}
}
