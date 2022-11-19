using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public class OptNotchOrderItem
	{
		public int NotchOrderItemId { get; set; }

		public int NotchOrderId { get; set; }

		public int PrintOrderId { get; set; }
		
		public OptNotchOrder NotchOrder { get; set; }

		public OptPrintOrder PrintOrder { get; set; }
	
	}
}
