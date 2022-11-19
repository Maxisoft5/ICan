namespace ICan.Common.Domain
{
	public partial class OptSemiproductWarehouseItem
	{
		public int SemiproductWarehouseItemId { get; set; }

		public int SemiproductWarehouseId { get; set; }
			    
		public int SemiproductId { get; set; }

		public int Amount { get; set; }

		public OptSemiproductWarehouse SemiproductWarehouse { get; set; }
		public OptSemiproduct Semiproduct { get; set; }
	}
}
