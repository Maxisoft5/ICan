namespace ICan.Common.Domain
{
	public partial class OptWarehouseItem
	{
		public int WarehouseItemId { get; set; }

		public int WarehouseId { get; set; }

		public int? ProductId { get; set; }

		public int? ObjectId { get; set; }

		public int Amount { get; set; }

		public OptWarehouse Warehouse { get; set; }
		public OptProduct Product { get; set; }
	}
}
