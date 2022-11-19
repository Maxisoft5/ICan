using System;

namespace ICan.Common.Models.Opt
{
	public class WarehouseItemModel
	{
		public int WarehouseItemId { get; set; }
		
		public int WarehouseId { get; set; }

		public int? ProductId { get; set; }

		public int? ObjectId { get; set; }

		public int Amount { get; set; }

		public string Shop { get; set; }

		public string Product { get; set; }
		public string ObjectDisplayName { get; set; }
		public DateTime? WarehouseDateAdd { get; set; }
	}
}
