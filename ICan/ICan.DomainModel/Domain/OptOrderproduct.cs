using System;

namespace ICan.Common.Domain
{
	public partial class OptOrderproduct
	{
		public int OrderProductId { get; set; }
		public Guid OrderId { get; set; }
		public int ProductId { get; set; }

		public int ProductPriceId { get; set; }

		public int Amount { get; set; }

		public OptOrder Order { get; set; }
		public OptProduct Product { get; set; }

		public OptProductprice ProductPrice { get; set; }
		public DateTime DateAdd { get; set; }
	}
}
