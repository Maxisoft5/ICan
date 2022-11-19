using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public class OptSiteFilter
	{
		public OptSiteFilter()
		{
			Products = new HashSet<OptSiteFilterProduct>();
		}

		public int SiteFilterId { get; set; }
		
		public int? SiteId { get; set; }

		public int? FilterCode { get; set; }

		public string Name { get; set; }

		public bool IsInternal { get; set; }
		
		public OptSite Site { get; set; }

		public ICollection<OptSiteFilterProduct> Products { get; set; }
	}
}
