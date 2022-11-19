using System.Collections.Generic;

namespace ICan.DomainModel.Jobs
{
	public class WbCheckWarehouseSetting
	{
		public string Timetable { get; set; }
		public int Limit { get; set; }
		public string Exchange { get; set; }
		public string Queue { get; set; }
		public string Mode { get; set; } //Dates or DaysGap
		public IEnumerable<WbCitySetting> Cities { get; set; }
	}
}
