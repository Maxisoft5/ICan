namespace ICan.Common.Models.Opt
{
	public class SemiproductWarehouseItemModel
	{
		public long SemiproductWarehouseItemId { get; set; }
		public long SemiproductWarehouseId { get; set; }

		public long SemiProductId { get; set; }

		public int Amount { get; set; } = 0;

		public string SemiproductName { get; set; }
	}
}
