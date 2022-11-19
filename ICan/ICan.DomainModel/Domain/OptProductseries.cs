namespace ICan.Common.Domain
{
	public partial class OptProductseries
	{
		public int ProductSeriesId { get; set; }

		public int ProductKindId { get; set; }

		public string Name { get; set; }
		
		public string SiteName { get; set; }

		public int? Order { get; set; }

	}
}
