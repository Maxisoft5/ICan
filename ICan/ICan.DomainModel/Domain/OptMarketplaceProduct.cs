using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public class OptMarketplaceProduct
	{
		public int MarketplaceProductId { get; set; }

		public int ProductId { get; set; }

		public int MarketplaceId { get; set; }

		public string Code { get; set; }
		public decimal? Price { get; set; }
		public float? Raiting { get; set; }
		public int? ReviewsAmount { get; set; }
		public bool ShowOnSite { get; set; }
		public ICollection<OptMarketplaceProductUrl> Urls{ get; set; }

		public OptProduct Product { get; set; }
		
		public OptMarketplace Marketplace { get; set; }
	}
}
