using Newtonsoft.Json;

namespace ICan.Common.Models.Mailchimp
{
	public class AddCustomField
	{
		[JsonProperty("list_id")]
		public string ListId { get; set; }
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("type")]
		public string Type { get; set; }
		[JsonProperty("tag")]
		public string Tag { get; set; }
		[JsonProperty("public")]
		public bool Public { get; set; } = true;
	}
}
