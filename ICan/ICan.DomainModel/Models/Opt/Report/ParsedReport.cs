namespace ICan.Common.Models.Opt.Report
{
    public record ParsedReport
    {
        public bool ShopIsFound { get; set; }
        public int ShopId { get; set; }
        public bool IgnoreUpd { get; set; }
        public ReportKind ReportKind { get; set; }
    }
}
