using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public class OptMarketplace
	{
		public OptMarketplace()
		{
			MarketplaceProducts = new HashSet<OptMarketplaceProduct>();
		}
		public int MarketplaceId { get; set; }
		public string Name { get; set; }
		public string ImageName { get; set; }
		public string Url { get; set; }
		public int SiteId { get; set; }
		
		public OptSite Site { get; set; }
		public ICollection<OptMarketplaceProduct> MarketplaceProducts { get; set; }
	}
}
