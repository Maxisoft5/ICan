using System;

namespace ICan.Common.Models.Opt
{
	public partial class ReportItemModel
	{
		public int ReportItemId { get; set; }
		public string ReportId { get; set; }
		public int? ShopId { get; set; }
		public int ProductId { get; set; }
		public int Amount { get; set; }
		public int ReportMonth { get; set; }
		public int ReportYear { get; set; }
		public int? ReportKindId { get; set; }
		public int ClientType { get; set; }
		public double TotalSum { get; set; }
		public double ReportTotalSum { get; set; }
		public Guid OrderId { get; set; }
	}
}
