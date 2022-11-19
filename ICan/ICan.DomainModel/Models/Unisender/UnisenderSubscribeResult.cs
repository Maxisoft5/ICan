using Newtonsoft.Json;

namespace ICan.Common.Models.Unisender
{
	public class UnisenderSubscribeResult
	{
		public int Total { get; set; }
		public int Inserted { get; set; }
		public int Updated { get; set; }
		public int Deleted { get; set; }
		[JsonProperty("new_emails")]
		public int NewEmails { get; set; }
		public int Invalid { get; set; }
		public Log[] Log { get; set; }
	}

	public class Log
	{
		public int Index { get; set; }
		public string Code { get; set; }
		public string Message { get; set; }
	}
}
