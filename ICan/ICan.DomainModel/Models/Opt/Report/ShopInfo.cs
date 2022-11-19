using System.Collections.Generic;

namespace ICan.Common.Models.Opt.Report
{
	public class ShopInfo
	{
		public int ShopId { get; set; }

		public int Month { get; set; }
		public int Year { get; set; }
		public double TotalSum { get; set; }
		public List<ShopItemInfo> ReportItems { get; set; } = new List<ShopItemInfo>();
		public int ReportKindId { get; set; }
	}

}
