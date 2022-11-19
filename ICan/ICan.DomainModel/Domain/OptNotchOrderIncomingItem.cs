namespace ICan.Common.Domain
{
	public class OptNotchOrderIncomingItem
	{
		public int NotchOrderIncomingItemId { get; set; }
		public int NotchOrderIncomingId { get; set; }
		public int? NotchOrderItemStickerId { get; set; }
		public int? SemiproductId { get; set; }
		public int Amount { get; set; }
		public bool IsAssembled { get; set; }

		public OptNotchOrderIncoming NotchOrderIncoming { get; set; }
		public OptSemiproduct Semiproduct { get; set; }
	}
}
