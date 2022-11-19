using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public partial class OptReportkind
	{
		public OptReportkind()
		{
			OptReport = new HashSet<OptReport>();
		}

		public int ReportKindId { get; set; }
		public string Name { get; set; }

		public ICollection<OptReport> OptReport { get; set; }
	}
}
