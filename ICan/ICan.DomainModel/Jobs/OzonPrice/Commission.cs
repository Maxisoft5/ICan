using Newtonsoft.Json;

namespace ICan.Common.Jobs.OzonPrice
{
	public class Commission
	{
		[JsonProperty("percent")]
		public int Percent { get; set; }

		[JsonProperty("min_value")]
		public int MinValue { get; set; }

		[JsonProperty("value")]
		public double Value { get; set; }

		[JsonProperty("sale_schema")]
		public string SaleSchema { get; set; }

		[JsonProperty("delivery_amount")]
		public int DeliveryAmount { get; set; }

		[JsonProperty("return_amount")]
		public int ReturnAmount { get; set; }
	}


}
