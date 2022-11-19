namespace ICan.Common.Domain
{
	public partial class OptShopName
	{
		public int ShopNameId { get; set; }

		public int ShopId { get; set; }

		public string Name { get; set; }
		public string Inn { get; set; }

		public OptShop Shop { get; set; }

		public bool Enabled { get; set; }
	}
}
