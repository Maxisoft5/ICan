using System;
using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public partial class OptPrintOrderIncoming
	{
		public OptPrintOrderIncoming()
		{
			PrintOrderIncomingItems = new HashSet<OptPrintOrderIncomingItem>();
		}
		public int PrintOrderIncomingId { get; set; }

		public int PrintOrderId { get; set; }

		public DateTime IncomingDate { get; set; }
		public int IncomingType { get; set; }
		public string Comment { get; set; }

		public OptPrintOrder PrintOrder { get; set; }

		public ICollection<OptPrintOrderIncomingItem> PrintOrderIncomingItems { get; set; }
	}
}
