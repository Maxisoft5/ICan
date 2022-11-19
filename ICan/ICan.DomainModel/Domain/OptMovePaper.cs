using System;

namespace ICan.Common.Domain
{
	public class OptMovePaper
	{
		public int MovePaperId { get; set; }
		public DateTime MoveDate { get; set; }
		public int SenderWarehouseId { get; set; }
		public int ReceiverWarehouseId { get; set; }
		public int SheetCount { get; set; }
		public double Weight { get; set; }
		public int PaperId { get; set; }
		public string Comment { get; set; }
		public int PrintOrderPaperId { get; set; }
		public OptPrintOrderPaper PrintOrderPaper { get; set; }
		public OptPaper Paper { get; set; }
		public OptWarehouseType SenderWarehouse { get; set; }
		public OptWarehouseType ReceiverWarehouse { get; set; }
	}
}
