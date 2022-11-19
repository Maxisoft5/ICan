namespace ICan.Common.Domain
{
	public partial class OptPrintOrderPaper
	{
		public int PrintOrderPaperId { get; set; }

		public int PaperOrderId { get; set; }

		public int PrintOrderId { get; set; }

		public int SheetsTakenAmount { get; set; }
	
		public bool IsSent { get; set; }

		public OptPrintOrder PrintOrder { get; set; }

		public OptPaperOrder PaperOrder { get; set; }
	}
}
