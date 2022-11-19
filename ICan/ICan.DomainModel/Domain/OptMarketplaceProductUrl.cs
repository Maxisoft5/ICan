namespace ICan.Common.Domain
{
	public class OptMarketplaceProductUrl
	{
		public int MarketplaceProductUrlId { get; set; }
		
		public int MarketplaceProductId { get; set; }
		
		public OptMarketplaceProduct MarketplaceProduct { get; set; }
		
		public string Url { get; set; }
	}
}
