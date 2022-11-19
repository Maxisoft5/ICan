using ICan.Common.Domain;
using ICan.Common.Models.Opt;
using ICan.Common.Utils;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace ICan.Common.Models
{
	public class ReportHelper
	{
		public int FromMonth { get; set; }
		public int ToMonth { get; set; }
		public int FromYear { get; set; }
		public int ToYear { get; set; }

		public int StartRow = 2;
		public int StartMonthColumn = 3;

		public DateTime StartDate => new DateTime(FromYear, FromMonth, 01);
		public DateTime EndDate => new DateTime(ToYear, ToMonth, 01).AddMonths(1);
		
		public IEnumerable<SelectListItem> Months => Util.GetAllMonths();

		public int[] ReportKinds { get; set; }

		public Dictionary<OptProductseries, List<ProductModel>> Products;
		public List<int> ShopIds { get; set; } = new List<int>();
        public Dictionary<int, int> ShopWithReportKind { get; set; } = new Dictionary<int, int>();
	}

}
