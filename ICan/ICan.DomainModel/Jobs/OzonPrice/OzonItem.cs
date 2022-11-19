using Newtonsoft.Json;
using System.Collections.Generic;

namespace ICan.Common.Jobs.OzonPrice
{
	public class OzonItem
	{
		[JsonProperty("product_id")]
		public int ProductId { get; set; }

		[JsonProperty("offer_id")]
		public string OfferISBN { get; set; }

		[JsonProperty("price")]
		public OzonPrice Price { get; set; }

		[JsonProperty("price_index")]
		public string PriceIndex { get; set; }

		[JsonProperty("commissions")]
		public List<Commission> Commissions { get; set; }

		[JsonProperty("marketing_actions")]
		public object MarketingActions { get; set; }

		[JsonProperty("volume_weight")]
		public double VolumeWeight { get; set; }
	}


}
