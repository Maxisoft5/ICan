using ICan.Common.Domain;
using System.Collections.Generic;

namespace ICan.Common.Models.Opt
{
	public class WarehouseStateModel  
	{
		public IEnumerable<KeyValuePair<OptProductseries, List<ProductModel>>> Items { get; set; }
	}
}
