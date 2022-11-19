using Newtonsoft.Json;

namespace ICan.Common.Jobs.OzonPrice
{
	public class Root
	{
		[JsonProperty("result")]
		public OzonResult Result { get; set; }
	}
}
