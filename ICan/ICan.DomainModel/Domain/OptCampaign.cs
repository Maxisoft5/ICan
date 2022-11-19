using System;

namespace ICan.Common.Domain
{
	public class OptCampaign
	{
		public int CampaignId { get; set; }
		public int CampaignType { get; set; }
		public string ExternalCampaignId { get; set; }
		public string Title { get; set; }
		public string Text { get; set; }
		public DateTime Date { get; set; } = DateTime.Now;
		public bool IsSent { get; set; }
		public string CampaignName { get; set; }
	}
}
