using System.Collections.Generic;

namespace ICan.Common.Models.Opt.Report
{
	public class ReportWarning
	{
		public int ShopId { get; set; }
		public string ShopName { get; set; }
		public List<ReportWarningField> Fields { get; set; } = new List<ReportWarningField>();
	}

	public class ReportWarningField
	{
		public ReportWarningField(string fieldName, string oldAddress, string newAddress)
		{
			FieldName = fieldName;
			OldAddress = oldAddress;
			NewAddress = newAddress;
		}
		public string FieldName { get; set; }
		public string OldAddress { get; set; }
		public string NewAddress { get; set; }
	}
}
