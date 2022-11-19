using System;
using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public partial class OptSemiproductWarehouse
	{
		public OptSemiproductWarehouse()
		{
			SemiproductWarehouseItems = new HashSet<OptSemiproductWarehouseItem>();
		}

		public int SemiproductWarehouseId { get; set; }

		public string Comment { get; set; }

		public DateTime Date { get; set; }

		public int WarehouseActionTypeId { get; set; }

		public OptWarehouseActionType WarehouseActionType { get; set; }
		public ICollection<OptSemiproductWarehouseItem> SemiproductWarehouseItems { get; set; }
	}
}
