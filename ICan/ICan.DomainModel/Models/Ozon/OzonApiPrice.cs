using Newtonsoft.Json;

namespace ICan.Common.Models.Ozon
{
	public class OzonApiPrice
	{
		[JsonProperty("price")]
		public double Price { get; set; }
		[JsonProperty("old_price")]
		public double? OldPrice { get; set; }
		[JsonProperty("premium_price")]
		public double? PremiumPrice { get; set; }
		[JsonProperty("recommended_price")]
		public double? RecommendedPrice { get; set; }
		
		[JsonProperty("retail_price")]
		public double? RetailPrice { get; set; }
		[JsonProperty("vat")]
		public double? Vat { get; set; }
	 
		[JsonProperty("marketing_price")]
		public double? MarketingPrice { get; set; }
		[JsonProperty("marketing_seller_price")]
		public double? MarketingSellerPrice { get; set; }
	}
}
