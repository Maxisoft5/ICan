using Newtonsoft.Json;

namespace ICan.Common.Models.Mailchimp
{
	public class AddContentToCampaignModel
	{
		[JsonProperty("campaign_id")]
		public string CampaignId { get; set; }
		[JsonProperty("html")]
		public string Html { get; set; }

	}
}
