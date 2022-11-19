using ICan.Common.Utils;

namespace ICan.Common.Models.Opt
{
	public class AssemblySemiproductModel
	{
		public int AssemblySemiproductId { get; set; }

		public int AssemblyId { get; set; }

		public int? PrintOrderSemiproductId { get; set; }

		public int? NotchOrderId { get; set; }
 
		public SemiProductType SemiproductTypeId { get; set; }

		public PrintOrderSemiproductModel PrintOrderSemiproduct { get; set; }
		public AssemblyModel Assembly { get; set; }

		public NotchOrderModel NotchOrder { get; set; }

		public int DisplayOrder => Util.GetOrderBySemiproductType((int)SemiproductTypeId);
	}
}