using System;
using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public class OptNotchOrder
	{
		public OptNotchOrder()
		{
			AssemblyNotchOrders = new HashSet<OptAssemblySemiproduct>();
		}

		public int NotchOrderId { get; set; }
		public string NotchOrderNumber { get; set; }
		public DateTime OrderDate { get; set; }

		public decimal? OrderSum { get; set; }
		public DateTime? ShipmentDate { get; set; }
		public decimal? ShipmentSum { get; set; }

		public ICollection<OptNotchOrderItem> NotchOrderItems { get; set; } = new HashSet<OptNotchOrderItem>();
		public ICollection<OptNotchOrderSticker> NotchOrderStickers { get; set; } = new HashSet<OptNotchOrderSticker>();
		public ICollection<OptNotchOrderIncoming> NotchOrderIncomings { get; set; } = new HashSet<OptNotchOrderIncoming>();

		public ICollection<OptAssemblySemiproduct> AssemblyNotchOrders { get; set; }
	}
}
