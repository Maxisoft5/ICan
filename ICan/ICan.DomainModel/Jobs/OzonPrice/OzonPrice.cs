using Newtonsoft.Json;

namespace ICan.Common.Jobs.OzonPrice
{
	public class OzonPrice
	{
		[JsonProperty("price")]
		public string Price { get; set; }

		[JsonProperty("old_price")]
		public string OldPrice { get; set; }

		[JsonProperty("premium_price")]
		public string PremiumPrice { get; set; }

		[JsonProperty("recommended_price")]
		public string RecommendedPrice { get; set; }

		[JsonProperty("retail_price")]
		public string RetailPrice { get; set; }

		[JsonProperty("vat")]
		public string Vat { get; set; }

		[JsonProperty("buybox_price")]
		public string BuyboxPrice { get; set; }

		[JsonProperty("min_ozon_price")]
		public string MinOzonPrice { get; set; }

		[JsonProperty("marketing_price")]
		public string MarketingPrice { get; set; }

		[JsonProperty("marketing_seller_price")]
		public string MarketingSellerPrice { get; set; }
	}


}
