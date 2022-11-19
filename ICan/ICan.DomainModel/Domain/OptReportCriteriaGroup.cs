using ICan.Common.Models.Opt.Report;
using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public class OptReportCriteriaGroup
	{
		public OptReportCriteriaGroup()
		{
			Criteria = new HashSet<OptReportCriteria>();
		}
		public int GroupId { get; set; }
		public int ShopId { get; set; }
		public byte Type { get; set; }
		public string GroupName { get; set; }
		public bool IsMain { get; set; }
		public ReportKind ReportKind { get; set; }
		public OptShop Shop { get; set; }
		public HashSet<OptReportCriteria> Criteria { get; set; }
	}
}
