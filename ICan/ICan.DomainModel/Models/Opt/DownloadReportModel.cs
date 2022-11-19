using System.ComponentModel.DataAnnotations;

namespace ICan.Common.Models.Opt
{
	public class DownloadReportModel
	{

		[Display(Name = "Месяц с")]
		public int FromMonth { get; set; }

		[Display(Name = "Месяц по")]
		public int ToMonth { get; set; }

		[Display(Name = "Год с")]
		public int FromYear { get; set; }

		[Display(Name = "Год по")]
		public int ToYear { get; set; }

	}
}
