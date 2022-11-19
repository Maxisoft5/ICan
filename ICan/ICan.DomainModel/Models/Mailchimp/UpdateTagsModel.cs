using Newtonsoft.Json;
using System.Collections.Generic;

namespace ICan.Common.Models.Mailchimp
{
	public class UpdateTagsModel
	{
		[JsonProperty("tags")]
		public IEnumerable<Tag> Tags { get; set; }
	}

	public class Tag
	{
		[JsonProperty("name")]
		public string TagName { get; set; }
		[JsonProperty("status")]
		public string Status { get; set; }
	}
}
