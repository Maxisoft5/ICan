using System;

namespace ICan.Common.Models.WB
{
	public interface IWbItemModel
	{
		DateTime Date { get; set; }
		int Quantity { get; set; }
		int ProductId { get; set; }
		string SupplierArticle { get; set; }
		string WarehouseName { get; set; }

		WbReportType ReportType { get; set; }
	}
}