namespace ICan.Common.Models.Opt
{
	public class PrintOrderIncomingItemModel
	{
		public int PrintOrderIncomingItemId { get; set; }
		public int PrintOrderIncomingId { get; set; }
		public int PrintOrderSemiproductId { get; set; }
		public int Amount { get; set; }

		public int ExistingAmount { get; set; }
		public bool Exists => ExistingAmount > 0;

		public string PrintOrderSemiproductName { get; set; }
		public int PrintOrderSemiproductTypeId { get; set; }
	}
}
