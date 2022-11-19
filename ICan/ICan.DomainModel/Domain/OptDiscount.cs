using System;
using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public partial class OptDiscount
	{
		public long DiscountId { get; set; }
	
		public double Value { get; set; }
		public bool Enabled { get; set; } = true;
		public bool IsArchived { get; set; }
		public string Description { get; set; }

		public DateTime CreateDate { get; set; }

		public ICollection<OptOrder> OptOrder { get; set; }
	}
}
