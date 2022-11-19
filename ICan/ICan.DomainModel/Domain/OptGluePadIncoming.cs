using System;

namespace ICan.Common.Domain
{
	public class OptGluePadIncoming
	{
		public int Id { get; set; }
		public string Description { get; set; }
		public double Price { get; set; }
		public int Amount { get; set; }
		public DateTime IncomingDate { get; set; }
	}
}
