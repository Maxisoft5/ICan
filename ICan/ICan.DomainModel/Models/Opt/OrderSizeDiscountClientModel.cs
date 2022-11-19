using ICan.Common.Domain;
using System.Collections.Generic;

namespace ICan.Common.Models.Opt
{
	public class OrderSizeDiscountClientModel
	{
		public int ClientType { get; set; }
		public IEnumerable<OptOrderSizeDiscount> OrderSizes { get; set; }
	}
}
