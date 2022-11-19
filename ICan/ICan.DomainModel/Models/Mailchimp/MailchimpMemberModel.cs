using Newtonsoft.Json;

namespace ICan.Common.Models.Mailchimp
{
	public class MailchimpMemberModel
	{
		[JsonProperty("email_address")]
		public string Email { get; set; }
		[JsonProperty("status")]
		public string Status { get; set; } = "subscribed";
		[JsonProperty("merge_fields")]
		public MergeFields MergeFields { get; set; }
		[JsonProperty("tags")]
		public string[] Tags { get; set; }
	}

	public class MergeFields
	{
		[JsonProperty("FNAME")]
		public string Name { get; set; }
		[JsonProperty("CLIENTTYPE")]
		public int ClientType { get; set; }
	}
}
