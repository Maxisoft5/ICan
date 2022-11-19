using ICan.Common.Models.Opt.Report;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ICan.Common.Models.WB
{
	public class WbFilterModel
	{

		[Display(Name = "Дата начала")]
		public DateTime StartDate { get; set; }
		[Display(Name = "Дата окончания")]
		public DateTime EndDate { get; set; }

		public WbReportTypeModel[] ReportTypes { get; set; }
		public IEnumerable<int> SelectedReportTypes => ReportTypes != null && ReportTypes.Any() ?
			ReportTypes.Where(report => report.IsSelected)
			.Select(report => (int)report.ReportType)
		 : Enumerable.Empty<int>();

		[Display(Name = "Тетрадь")]
		public WbReportProductModel[] Products { get; set; }
		public IEnumerable<int> SelectedProducts => Products != null && Products.Any() ?
				Products.Where(product => product.IsSelected).Select(product => product.ProductId)
			 : Enumerable.Empty<int>();

		[Display(Name = "Склад")]
		public WbReportWarehouseModel[] WarehouseNames { get; set; }

		public IEnumerable<string> SelectedWarehouseNames => WarehouseNames != null && WarehouseNames.Any() ?
			WarehouseNames.Where(whName => whName.IsSelected).Select(whName => whName.DisplayName)
		 : Enumerable.Empty<string>();
	}
}
