using Newtonsoft.Json;
using System.Collections.Generic;

namespace ICan.Common.Jobs.OzonPrice
{
	public class OzonResult
	{
		[JsonProperty("items")]
		public List<OzonItem> Items { get; set; }

		[JsonProperty("total")]
		public int Total { get; set; }
	}


}
