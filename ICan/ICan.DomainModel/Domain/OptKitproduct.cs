namespace ICan.Common.Domain
{
	public partial class OptKitproduct
	{
		public int KitProductId { get; set; }
		public int ProductId { get; set; }
		public int MainProductId { get; set; }

		public int OrderNum { get; set; }

		public OptProduct Product { get; set; }
		public OptProduct MainProduct { get; set; }
		// public OptProduct MainProduct { get; set; }
	}
}
