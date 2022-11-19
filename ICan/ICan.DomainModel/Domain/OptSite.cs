using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public class OptSite
	{
		public int SiteId { get; set; }
		public string Name { get; set; }
		public string Url { get; set; }
		public string Locale { get; set; }
		public ICollection<OptSiteFilter> SiteFilters { get; set; }
		public ICollection<OptMarketplace> Marketplaces { get; set; }
	}
}
