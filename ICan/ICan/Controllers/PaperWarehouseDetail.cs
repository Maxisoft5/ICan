using System.Collections.Generic;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class PaperWarehouseDetail
	{
		public int PaperId { get; set; }

		public string Name { get; set; }

		public IEnumerable<CalcPaperWhjDetails> Warehouses { get; set; }

		public int? Amount => Warehouses?.Sum(wh => wh.Current);
	}
}