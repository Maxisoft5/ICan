using System;

namespace ICan.Common.Domain
{
	public class OptUtmStatistics
	{
		public int StatisticsId { get; set; }

		public DateTime Date { get; set; }

		public string UtmSource { get; set; }
		public string UtmMedium { get; set; }
		public string UtmCampaign { get; set; }
		public string UtmContent { get; set; }
		public string UtmTerm { get; set; }
	}
}
