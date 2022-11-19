using System.Collections.Generic;

namespace ICan.Common.Models.Opt
{
	public class TableDataResult<T> where T: class 
	{
		public int Total { get; set; }
		public IEnumerable<T> Rows { get; set; }
	}
}
