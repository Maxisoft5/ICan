using System;
using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public class OptNotchOrderIncoming
	{
		public int NotchOrderIncomingId { get; set; }
		public DateTime IncomingDate { get; set; }
		public int NotchOrderId { get; set; }
		public ICollection<OptNotchOrderIncomingItem> IncomingItems { get; set; } = new HashSet<OptNotchOrderIncomingItem>();
		public OptNotchOrder NotchOrder { get; set; }
	}
}
