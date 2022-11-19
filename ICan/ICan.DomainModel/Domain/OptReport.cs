using System;
using System.Collections.Generic;

namespace ICan.Common.Domain
{
    public partial class OptReport
    {
        public OptReport()
        {
            ReportItems = new HashSet<OptReportitem>();
        }

        public string ReportId { get; set; }
        public int ReportKindId { get; set; }
        public int ShopId { get; set; }
        public double TotalSum { get; set; }
        public double? PaidSum { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime? ReportDate { get; set; }
        public int ReportMonth { get; set; }
        public int ReportYear { get; set; }
        public string ReportNum { get; set; }
        public string FileName { get; set; }
        public DateTime? ReportPeriodFrom { get; set; }
        public DateTime? ReportPeriodTo { get; set; }


        public OptReportkind ReportKind { get; set; }
        public OptShop Shop { get; set; }
        public ICollection<OptReportitem> ReportItems { get; set; }
        public bool? IsVirtual { get; set; }
        public bool IsArhived { get; set; }
    }
}
