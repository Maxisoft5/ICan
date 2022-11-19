using System.Collections.Generic;

namespace ICan.Common.Domain
{
	public partial class OptProduct
	{
		public OptProduct()
		{
			OptOrderproduct = new HashSet<OptOrderproduct>();
			ProductPrices = new HashSet<OptProductprice>();
			OptWarehouseItem = new HashSet<OptWarehouseItem>();
			KitProducts = new HashSet<OptKitproduct>();
			Semiproducts = new HashSet<OptSemiproduct>();
			Assemblies = new HashSet<OptAssembly>();
			ProductTags = new HashSet<OptProductTag>();
			ProductImages = new HashSet<OptProductImage>();
			MarketplaceProducts = new HashSet<OptMarketplaceProduct>();
			RelatedSemiproducts = new HashSet<OptSemiproductProductRelation>();
			SiteFilterProducts = new HashSet<OptSiteFilterProduct>();
		}

		public int ProductId { get; set; }

		public string Name { get; set; }
		public string SiteName { get; set; }


		public bool ShowPreviousPrice { get; set; }

		public int ProductKindId { get; set; }

		public bool IsKit { get; set; }

		public bool Enabled { get; set; }

		public bool IsArchived { get; set; }

		public int? ProductSeriesId { get; set; }

		public float Weight { get; set; }

		public string ISBN { get; set; }

		public string ArticleNumber { get; set; }

		public int? DisplayOrder { get; set; }
		public int? CountryId { get; set; }
		public string RegionalName { get; set; }
	
		public OptCountry Country { get; set; }

		public OptProductkind ProductKind { get; set; }

		public OptProductseries ProductSeries { get; set; }
		public ICollection<OptOrderproduct> OptOrderproduct { get; set; }

		public ICollection<OptProductprice> ProductPrices { get; set; }

		public ICollection<OptWarehouseItem> OptWarehouseItem { get; set; }

		public ICollection<OptKitproduct> KitProducts { get; set; }

		public OptKitproduct  KitPart { get; set; }

		public ICollection<OptSemiproduct> Semiproducts { get; set; }
		public ICollection<OptAssembly> Assemblies { get; set; }

		public string VideoFileName { get; set; }

		public string Description { get; set; }
		public string LongDescription { get; set; }
		public string Information { get; set; }

		public string BotInformation { get; set; }
		public string BotDescription { get; set; }
		public string ReviewsText { get; set; }


		public ICollection<OptProductTag> ProductTags { get; set; }
		public ICollection<OptProductImage> ProductImages { get; set; }
		public ICollection<OptMarketplaceProduct> MarketplaceProducts { get; set; }
		public ICollection<OptSemiproductProductRelation> RelatedSemiproducts { get; set; }
		public ICollection<OptSiteFilterProduct> SiteFilterProducts { get; set; }
	}
}
