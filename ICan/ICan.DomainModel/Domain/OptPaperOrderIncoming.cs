using System;

namespace ICan.Common.Domain
{
	public class OptPaperOrderIncoming
	{
		public int PaperOrderIncomingId { get; set; }
		public int PaperOrderId { get; set; }
		public int Amount { get; set; }
		public DateTime IncomingDate { get; set; }
		public int WarehouseTypeId { get; set; }

		public OptPaperOrder PaperOrder { get; set; }
		
		public OptWarehouseType WarehouseType { get; set; }
	}
}
