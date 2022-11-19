using System;
using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Domain
{
	public partial class OptOrderpayment
	{
		public int OrderPaymentId { get; set; }
		public Guid OrderId { get; set; }
		[Display(Name = "Дата платежа")]
		[DataType(DataType.Date)]
		public DateTime OrderPaymentDate { get; set; }

		[Display(Name = "Сумма, руб")]
		public double Amount { get; set; }


		public OptOrder Order { get; set; }
	}
}
