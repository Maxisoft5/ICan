using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public partial class OptShop
	{
		public OptShop()
		{
			Report = new HashSet<OptReport>();
			ShopNames = new HashSet<OptShopName>();
			ShopNames = new HashSet<OptShopName>();
			Orders = new HashSet<OptOrder>();
		}

		public int ShopId { get; set; }
		public string Name { get; set; }
		public string Consignee { get; set; }
		public string ShopUrl { get; set; }
		public bool Enabled { get; set; }
		public bool? IsMarketPlace { get; set; }
		public bool? NonResident { get; set; }
		public bool IgnoreInWarehouseCalc { get; set; }
		public short? Postponement { get; set; }
		public string ScanFileName { get; set; }
		public string MimeType { get; set; }
		public ICollection<OptReportCriteriaGroup> CriteriaGroups { get; set; }
		public ICollection<OptReport> Report { get; set; }
		public ICollection<OptShopName> ShopNames { get; set; }
		public ICollection<OptUpdPayment> UpdPayments { get; private set; }
		public ICollection<OptApplicationUserShopRelation> ApplicationUserShopRelations { get; set; }
		public ICollection<OptOrder> Orders { get; set; }
	}
}
