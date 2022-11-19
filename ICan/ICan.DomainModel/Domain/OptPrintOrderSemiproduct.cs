using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public partial class OptPrintOrderSemiproduct
	{
		public OptPrintOrderSemiproduct()
		{
			PrintOrderIncomingItems = new HashSet<OptPrintOrderIncomingItem>();
			AssemblySemiproducts = new HashSet<OptAssemblySemiproduct>();
		}

		public int PrintOrderSemiproductId { get; set; }

		public int PrintOrderId { get; set; }

		public int SemiproductId { get; set; }
		public bool  IsAssembled { get; set; }

		public OptPrintOrder PrintOrder { get; set; }

		public OptSemiproduct SemiProduct { get; set; }

		public ICollection<OptPrintOrderIncomingItem> PrintOrderIncomingItems { get; set; }

		public ICollection<OptAssemblySemiproduct> AssemblySemiproducts { get; set; }
	}
}
