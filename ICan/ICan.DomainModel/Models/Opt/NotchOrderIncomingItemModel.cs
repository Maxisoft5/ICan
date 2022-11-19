namespace ICan.Common.Models.Opt
{
	public class NotchOrderIncomingItemModel
	{
		public int NotchOrderIncomingItemId { get; set; }
		public int NotchOrderIncomingId { get; set; }
		public int? SemiproductId { get; set; }
		public int? NotchOrderItemStickerId { get; set; }
		public int Amount { get; set; }
		public bool IsAssembled { get; set; }

		public NotchOrderIncomingModel NotchOrderIncoming { get; set; }
		public SemiproductModel Semiproduct { get; set; }

		public string NotchOrderSemiproduct => Semiproduct != null ? Semiproduct.DisplayName : "";

		public int NotchOrderStickerId { get; set; }		
	}
}
