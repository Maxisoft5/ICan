namespace ICan.Common.Domain
{
	public class OptAssemblySemiproduct
	{
		public int AssemblySemiproductId { get; set; }
		
		public int AssemblyId { get; set; }
		
		public int? PrintOrderSemiproductId { get; set; }
		
		public int? NotchOrderId { get; set; }
		 
		public OptAssembly Assembly { get; set; }

		public OptPrintOrderSemiproduct PrintOrderSemiproduct { get; set; }
		
	 	public OptNotchOrder NotchOrder { get; set; }
	}
}