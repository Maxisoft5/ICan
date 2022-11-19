using Newtonsoft.Json;

namespace ICan.Common.Models.Unisender
{
	public class ResultId
	{
		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("message_id")]
		private int MessageId { set { Id = value; } }
	}
}
