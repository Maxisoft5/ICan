using System;

namespace ICan.Common.Models.Opt
{
	public class PrintOrderSemiproductModel
	{
		public int PrintOrderSemiproductId { get; set; }
		public int PrintOrderId { get; set; }
		public DateTime PrintOrderOrderDate { get; set; }
		public string PrintOrderPrintingHouseOrderNum { get; set; }
		public int SemiproductId { get; set; }
		public SemiproductModel SemiProduct { get; set; }
		public bool IsAssembled { get; set; }

		public string DisplayName
		{
			get
			{
				return $"{PrintOrderOrderDate.ToShortDateString()} {PrintOrderPrintingHouseOrderNum}";
			}
		}
	}
}
