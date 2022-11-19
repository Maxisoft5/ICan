using System;

namespace ICan.Common.Domain
{
	public partial class OptOrderSizeDiscount
	{
		public int OrdersizeDiscountId { get; set; }

		public double From { get; set; }
		public double To { get; set; }
		public double DiscountPercent { get; set; }
		public DateTime? DateStart { get; set; }

		public DateTime? DateEnd { get; set; }

		public int ClientType { get; set; }
	}
}
