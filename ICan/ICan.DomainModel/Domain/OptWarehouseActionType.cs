using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public partial class OptWarehouseActionType
	{
		public OptWarehouseActionType()
		{
			Warehouses = new HashSet<OptWarehouse>();
			SemiproductWarehouses = new HashSet<OptSemiproductWarehouse>();
		}

		public int WarehouseActionTypeId { get; set; }

		public string Name { get; set; }
		public ICollection<OptWarehouse> Warehouses { get; set; }
		public ICollection<OptSemiproductWarehouse> SemiproductWarehouses { get; set; }
	}
}
