namespace ICan.Common.Domain
{
	public partial class OptPrintOrderIncomingItem
	{
		public int PrintOrderIncomingItemId { get; set; }

		public int PrintOrderIncomingId { get; set; }

		public int PrintOrderSemiproductId { get; set; }

		public int Amount { get; set; }

		public OptPrintOrderIncoming PrintOrderIncoming { get; set; }

		public OptPrintOrderSemiproduct PrintOrderSemiproduct { get; set; }
	}
}
