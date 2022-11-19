namespace ICan.Common.Domain
{
	public class OptSiteFilterProduct
	{
		public int SiteFilterProductId { get; set; }
	
		public int SiteFilterId { get; set; }
		
		public int ProductId   { get; set; }

		public int? Order { get; set; }

		public OptSiteFilter SiteFilter{ get; set; }
		
		public OptProduct Product{ get; set; }

	}
}
