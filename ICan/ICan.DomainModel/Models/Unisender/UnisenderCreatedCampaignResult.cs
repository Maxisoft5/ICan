using Newtonsoft.Json;

namespace ICan.Common.Models.Unisender
{
	public class UnisenderCreatedCampaignResult
	{
		[JsonProperty("campaign_id")]
		public int CampaignId { get; set; }
		public string Status { get; set; }
		public int Count { get; set; }
		public string Error { get; set; }
	}
}
