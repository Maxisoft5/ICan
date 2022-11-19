using ICan.Common.Models.Enums;
using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public class OptSpring
	{
		public int SpringId { get; set; }
		public string SpringName { get; set; }
		public int BlockThickness { get; set; }
		public StepEnum Step { get; set; }
		public int NumberOfTurnsId { get; set; }
		public OptNumberOfTurns NumberOfTurns { get; set; }
		public IEnumerable<OptSpringOrder> SpringOrders { get; set; }
	}
}
