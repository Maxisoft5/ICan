using System;

namespace ICan.Common.Domain
{
	public class OptWbSale : IWBItemWithProductId
	{
		public long WbSaleId { get; set; }
		public string Number { get; set; }
		public DateTime Date { get; set; }
		public DateTime LastChangeDate { get; set; }
		public string SupplierArticle { get; set; }
		public string TechSize { get; set; }
		public string Barcode { get; set; }
		public int Quantity { get; set; }
		public decimal TotalPrice { get; set; }
		public decimal DiscountPercent { get; set; }
		public bool IsSupply { get; set; }
		public bool IsRealization { get; set; }
		public long? OrderId { get; set; }
		public int PromoCodeDiscount { get; set; }
		public string WarehouseName { get; set; }
		public string CountryName { get; set; }
		public string OblastOkrugName { get; set; }
		public string RegionName { get; set; }
		public string SaleId { get; set; }
		public int? IncomeId { get; set; }
		
		public long Odid { get; set; }
		public int NmId { get; set; }
		public int Spp { get; set; }
		public string Subject { get; set; }
		public string Category { get; set; }
		public string Brand { get; set; }
		public double ForPay { get; set; }
		public double FinishedPrice { get; set; }
		public double PriceWithDisc { get; set; }
	
		public int IsStorno { get; set; }
		public string GNumber { get; set; }
		public int? ProductId { get; set; }
		public DateTime UploadDate{ get; set; }
	}
}
