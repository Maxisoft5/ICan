using System;

namespace ICan.Common.Models.WB
{
	public class WeeklyRepotDataItem
	{
	
		public static readonly string WeekNumCell = "B"; 
		public static readonly string SaleDateCell = "C";
		public static readonly string ArticleNumberCell = "D";
		public static readonly string SoldAmountCell = "F";
		public static readonly string AwardSumCell = "G";
		public static readonly string OrderedAmountCell = "H";
		public static readonly string OrderedSumCell = "I";

		public DateTime SaleDate { get; set; }
		public int WeekNum { get; set; }
		public string ArticleNumber { get; set; }
		public string ISBN { get; set; }
		public int ProductId { get; set; }
		public int SoldAmount { get; set; }
		public decimal AwardSum { get; set; }
		public int OrderedAmount { get; set; }
		public decimal OrderedSum { get; set; }
	}
}
