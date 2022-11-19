namespace ICan.Common.Domain
{
	public class OptReportCriteria
	{
		public int CriteriaId { get; set; }
		public int GroupId { get; set; }
		public string Address { get; set; }
		public string Information { get; set; }
		public OptReportCriteriaGroup Group { get; set; }
	}
}
