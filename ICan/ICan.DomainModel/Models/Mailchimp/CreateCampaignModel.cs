using Newtonsoft.Json;

namespace ICan.Common.Models.Mailchimp
{
	public class CreateCampaignModel
	{
		[JsonProperty("type")]
		public string Type { get; set; } = "regular";
		[JsonProperty("recipients")]
		public Recipients Recipients { get; set; }
		[JsonProperty("settings")]
		public CampaignSettings Settings { get; set; }
	}

	public class CampaignSettings
	{
		[JsonProperty("subject_line")]
		public string Subject { get; set; }
		[JsonProperty("title")]
		public string Title { get; set; }
		[JsonProperty("reply_to")]
		public string EmailForReply { get; set; }
		[JsonProperty("from_name")]
		public string FromName { get; set; }
	}

	public class Recipients
	{
		[JsonProperty("list_id")]
		public string ListId { get; set; }
		[JsonProperty("segment_opts")]
		public SegmentOptionsCampaign SegmentOptions { get; set; }
	}

	public class SegmentOptionsCampaign
	{
		[JsonProperty("saved_segment_id")]
		public int SegmentId { get; set; }
	}
}
