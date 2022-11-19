using System;
using System.Collections.Generic;

namespace ICan.Common.Models.WB
{
	public class WarehouseReport
	{
		public IEnumerable<IWbItemModel> Orders { get; set; }
		public IEnumerable<IWbItemModel> WarehouseItems { get; set; }

		public DateTime MinDate { get; set; }
		public DateTime MaxDate { get; set; }
	}
}
