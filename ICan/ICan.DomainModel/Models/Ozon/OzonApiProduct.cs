using Newtonsoft.Json;

namespace ICan.Common.Models.Ozon
{
	public class OzonApiProduct
	{
		[JsonProperty("product_id")]
		public int ProductId { get; set; }

		[JsonProperty("offer_id")]
		public string OfferId { get; set; }

		[JsonProperty("price")]
		public OzonApiPrice Price { get; set; }

		[JsonProperty("price_index")]
		public double Priceindex { get; set; }

		[JsonProperty("volume_weight")]
		public double VolumeWeight { get; set; }
	}
}
