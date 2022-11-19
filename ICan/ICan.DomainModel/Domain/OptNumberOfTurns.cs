using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public class OptNumberOfTurns
	{
		public int NumberOfTurnsId { get; set; }
		public int NumberOfTurns { get; set; }
		public string Manufacturer { get; set; }
		public IEnumerable<OptSpring> Springs { get; set; }
	}
}
