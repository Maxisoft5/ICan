namespace ICan.Common.Models.Opt
{
	public class NotchOrderItemModel
	{
		public int NotchOrderItemId { get; set; }
		public int NotchOrderId { get; set; }
		public int PrintOrderId { get; set; }
		public NotchOrderModel NotchOrder { get; set; }
		public PrintOrderModel PrintOrder { get; set; }
	}
}
