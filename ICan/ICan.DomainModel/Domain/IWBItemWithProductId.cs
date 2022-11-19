using System;

namespace ICan.Common.Domain
{
	public interface IWBItemWithProductId
	{
		string Number { get; set; }

		DateTime LastChangeDate { get; set; }

		DateTime Date { get; set; }
		DateTime UploadDate { get; set; }
		string Barcode { get; set; }
		string SupplierArticle { get; set; }
		int? ProductId { get; set; }

		int Quantity { get; set; }
		string WarehouseName { get; set; }

		int? IncomeId { get; set; }
		
		long Odid { get; set; }
		int NmId { get; set; }
	
		string Subject { get; set; }
		public string Category { get; set; }
		public string Brand { get; set; }
	}
}