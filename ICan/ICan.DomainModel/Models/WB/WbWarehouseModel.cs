using System;

namespace ICan.Common.Models.WB
{
	public class WbWarehouseModel
	{
		public long WbWarehouseId { get; set; }
		public DateTime LastChangeDate { get; set; }
		public string SupplierArticle { get; set; }
		public string TechSize { get; set; }
		public string Barcode { get; set; }
		public int Quantity { get; set; }
		public bool IsSupply { get; set; }
		public bool IsRealization { get; set; }
		public int QuantityFull { get; set; }
		public int QuantityNotInOrders { get; set; }
		public string WarehouseName { get; set; }
		public int InWayToClient { get; set; }
		public int InWayFromClient { get; set; }
		public int NmId { get; set; }
		public string Subject { get; set; }
		public string Category { get; set; }
		public int DaysOnSite { get; set; }
		public string Brand { get; set; }
		public string SCCode { get; set; }
		public decimal Price { get; set; }
		public decimal Discount { get; set; }
		public int ProductId { get; set; }
	}
}
