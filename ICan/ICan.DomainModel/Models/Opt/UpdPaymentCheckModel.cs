using System;
using System.Collections.Generic;

namespace ICan.Common.Models.Opt
{
	public class UpdPaymentCheckModel
	{
		public string ReportId { get; set; }
		public List<string> ReportNums { get; set; }
		public string DisplayReportNums => string.Join(",", ReportNums);
		public DateTime ReportDate { get; set; }
		public string ShopName { get; set; }
		public double TotalSum { get; set; }
		public bool IsPaid { get; set; }
		public DateTime? CheckDate { get; set; }
		public short Postponement { get; set; }
		public int ShopId { get; set; }

		public DateTime ExpectingPaymentDate => ReportDate.AddDays(Postponement);
		public string ExpectingPaymentMontYear => $"{ExpectingPaymentDate.Month:D2}.{ExpectingPaymentDate.Year.ToString().Substring(2)}";


	}
}
