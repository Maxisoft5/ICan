using System;
using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public class OptWarehouse
	{
		public OptWarehouse()
		{
			WarehouseItems = new HashSet<OptWarehouseItem>();
		}

		public int WarehouseId { get; set; }
		
		public DateTime DateAdd { get; set; }
		
		public string Comment { get; set; }
		
		public int WarehouseActionTypeId { get; set; }

		public int WarehouseTypeId { get; set; }

		public OptWarehouseType WarehouseType { get; set; }
		
		public OptAssembly Assembly { get; set; }

		public OptWarehouseActionType WarehouseActionType { get; set; }

		public ICollection<OptWarehouseItem> WarehouseItems { get; set; }
	}
}
