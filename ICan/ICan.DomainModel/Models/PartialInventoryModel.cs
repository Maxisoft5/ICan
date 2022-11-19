using ICan.Common.Models.Opt;
using System.Collections.Generic;

namespace ICan.Common.Models
{
	public class PartialInventoryModel
	{
		public IEnumerable<SemiproductModel> SemiproductList { get; set; }
		public int SemiproductId { get; set; }
		public int WarehouseTypeId { get; set; }
		public int Amount { get; set; }
	}	
}
