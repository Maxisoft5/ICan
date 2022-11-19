namespace ICan.Common.Models.Opt
{
	public class PrintOrderPaperModel
	{
		public int PrintOrderPaperId { get; set; }
		public int PaperOrderId { get; set; }
		public int PrintOrderId { get; set; }

		public int SheetsTakenAmount { get; set; }

		public string PrintOrderInfo { get; set; }
		public string PaperOrder { get; set; }

		public bool IsSent { get; set; }

		public PrintOrderModel PrintOrder { get; set; }
	}
}
