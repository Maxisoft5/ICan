using System;
using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public partial class OptEvent
	{
		public int EventId { get; set; }

		public string Name { get; set; }

		public DateTime? StartDate { get; set; }

		public DateTime? EndDate { get; set; }

		public bool Enabled { get; set; }

		public bool IsDeleted { get; set; }

		public float DiscountPercent { get; set; }

		public string Description { get; set; }

		public ICollection<OptOrder> OptOrder { get; set; }
	}
}
