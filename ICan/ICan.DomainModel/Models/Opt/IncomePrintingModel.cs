namespace ICan.Common.Models.Opt
{
	public partial class PrintOrderModel
	{
		public class IncomePrintingModel
		{
			public SemiProductType SemiProductTypeId { get; set; }
			public int MinIncome { get; set; }
			public int PrintingToCheck { get; set; }
		}
	}
}
