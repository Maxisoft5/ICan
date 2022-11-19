using System;

namespace ICan.Common.Domain
{
	public class OptSpringOrderIncoming
	{
		public int SpringOrderIncomingId { get; set; }
		public int SpoolCount { get; set; }
		public int NumberOfTurnsCount { get; set; }
		public DateTime IncomingDate { get; set; }
		public int SpringOrderId { get; set; }
		public OptSpringOrder SpringOrder { get; set; }
	}
}
