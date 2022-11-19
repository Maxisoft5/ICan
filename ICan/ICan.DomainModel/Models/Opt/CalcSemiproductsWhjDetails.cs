using System.Collections.Generic;

namespace ICan.Common.Models.Opt
{
	public class CalcSemiproductsWhjDetails
	{
		public long ProductId { get; set; }
		public string ProductName { get; set; }
		public string SeriesName { get; set; }
		public List<SemiproductPartDetails> SemiproductList { get; set;  } = new List<SemiproductPartDetails>();		
	}	
}
