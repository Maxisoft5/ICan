using System;

namespace ICan.Common.Models.WB
{
	public class WbOrderModel : IWbItemModel
	{
		public long WbOrderId { get; set; }
		public string Number { get; set; }
		public DateTime Date { get; set; }
		public DateTime LastChangeDate { get; set; }
		public string SupplierArticle { get; set; }
		public string TechSize { get; set; }
		public string Barcode { get; set; }
		public int Quantity { get; set; }
		public decimal TotalPrice { get; set; }
		public decimal DiscountPercent { get; set; }
		public string WarehouseName { get; set; }

		public string CountryName { get; set; }
		public string Oblast  { get; set; }
		public int? IncomeId { get; set; }
		public long Odid { get; set; }
		public int NmId { get; set; }
		public string Subject { get; set; }
		public string Category { get; set; }
		public string Brand { get; set; }
		public bool IsCancel { get; set; }
		public DateTime? CancelDate { get; set; }

		public int ProductId { get; set; }
		public DateTime UploadDate { get; set; }
		public WbReportType ReportType { get; set; }
	}
}
