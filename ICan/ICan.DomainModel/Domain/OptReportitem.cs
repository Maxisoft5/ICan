namespace ICan.Common.Domain
{
	public partial class OptReportitem
	{
		public int ReportItemId { get; set; }
		public string ReportId { get; set; }
		public int ProductId { get; set; }
		public int Amount { get; set; }
		public double TotalSum { get; set; }
		public OptReport Report { get; set; }
	}
}
