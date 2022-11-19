using Newtonsoft.Json;
using System.Collections.Generic;

namespace ICan.Common.Models.Mailchimp
{
	public class CreateSegmentModel
	{
		[JsonProperty("list_id")]
		public string List_Id { get; set; }
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("options")]
		public SegmentOptions Options { get; set; }
	}

	public class SegmentOptions
	{
		[JsonProperty("match")]
		public string Match => "all";
		[JsonProperty("conditions")]
		public IEnumerable<SegmentCondition> Conditions { get; set; }
	}

	public class SegmentCondition
	{
		[JsonProperty("condition_type")]
		public string ConditionType { get; set; }
		[JsonProperty("field")]
		public string Field { get; set; }
		[JsonProperty("op")]
		public string Option { get; set; }
		[JsonProperty("value")]
		public string Value { get; set; }
	}
}
