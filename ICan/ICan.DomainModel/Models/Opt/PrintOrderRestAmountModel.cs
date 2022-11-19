using System;

namespace ICan.Common.Models.Opt
{
	public class PrintOrderRestAmountModel
	{
		public int RestAmount { get; set; }
		public long PrintOrderId { get; set; }
		public string PrintingHouseOrderNum { get; set; }
		public DateTime OrderDate { get; set; }
		public int Printing { get; set; }
	}
}