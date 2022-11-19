using Newtonsoft.Json;
using System.Collections.Generic;

namespace ICan.Common.Models.Mailchimp
{
	public class Segments
	{
		[JsonProperty("segments")]
		public IEnumerable<SegmentModel> SegmentsList { get; set; }
	}

	public class SegmentModel
	{
		[JsonProperty("id")]
		public int SegmentId { get; set; }
		public string Name { get; set; }
		[JsonProperty("list_id")]
		public string ListId { get; set; }
	}
}
