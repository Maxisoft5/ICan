using ICan.Common.Domain;
using System.Collections.Generic;

namespace ICan.Common.Models.Opt.Report
{
	public class PrimerReportHelper
	{
		public int Year { get; set; }
		public int ProductId { get; set; }
		public List<OptOrder> SPOrders { get; set; }
		public List<OptOrder> ShopOrders { get; set; }
		public List<OptReportitem> OzonReportItems { get; set; }
		public List<OptReportitem> WBReportItems { get; set; }
		public List<OptReportitem> UPDReportItems { get; set; }
		public List<OptWarehouse> whJournalItems { get; set; }
	}
}
