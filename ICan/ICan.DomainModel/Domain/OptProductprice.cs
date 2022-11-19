using ICan.Common.Models;
using System;
using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public class OptProductprice
	{
		public int ProductPriceId { get; set; }
		public int ProductId { get; set; }

		public double Price { get; set; }

		public DateTime? DateStart { get; set; }

		public DateTime? DateEnd { get; set; }

		public OptProduct Product { get; set; }

		public ICollection<OptOrderproduct> OptOrderproduct { get; set; }
	}
}
