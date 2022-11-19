using System;
using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public class OptAssembly
	{
		public int AssemblyId { get; set; }
	
		public DateTime Date { get; set; }

		public int ProductId { get; set; }
		public int? WarehouseId { get; set; }

		public int Amount { get; set; }

		public int AssemblyType { get; set; }

		public OptProduct Product { get; set; }
		public OptWarehouse Warehouse { get; set; }

		public ICollection<OptAssemblySemiproduct> AssemblySemiproducts { get; set; }
	}
}
