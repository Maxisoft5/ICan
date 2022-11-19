using ICan.Common.Models.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class SiteProductModel
	{	
		public int ProductId { get; set; }

		[Display(Name = "Название")]

		public string Name { get; set; }
		[Display(Name = "Название для сайта")]
		public string SiteName { get; set; }
		
		public string DisplayPrice
		{
			get
			{
				var price = MarketplaceProducts?.Select(prod => prod.Price).Min();

				if (price.HasValue)
				{
					var mPrice = MarketplaceProducts.First(m => m.Price == price);
					return mPrice.DisplayPriceAndCurrency;
				}
				return "-";
			}
		}
		public string DisplayName => string.IsNullOrWhiteSpace(CountryPrefix)
			? Name
			: $"{CountryPrefix} {Name}";


		[Display(Name = "Порядок отображения в модуле")]
		public int? DisplayOrder { get; set; }

		[Display(Name = "Видеофайл")]
		public string VideoFileName { get; set; }

		public string BucketUrl { get; set; }

		public string VideoFileFullPath => !string.IsNullOrWhiteSpace(VideoFileName)
			? $"{BucketUrl}/{VideoFileName}"
			: string.Empty;

		[Display(Name = "Серия")]
		public int ProductSeriesId { get; set; }
		[Display(Name = "Серия")]
		public string ProductSeriesSiteName { get; set; }
		public string RegionalName { get; set; }

		public int? CountryId { get; set; }

		public string ForeignDisplayName => !CountryId.HasValue
		? Name
		: $"{RegionalName}({Name})";

		public IEnumerable<ProductTagModel> ProductTags { get; set; }

		public IEnumerable<MarketplaceProductModel> MarketplaceProducts { get; set; }
		public MarketplaceProductModel WbMarketPlaceProduct =>
			MarketplaceProducts?.FirstOrDefault(prod => prod.MarketplaceId == (int)Marketplace.WB);
		public MarketplaceProductModel YaMarketPlaceProduct =>
			MarketplaceProducts?.FirstOrDefault(prod => prod.MarketplaceId == (int)Marketplace.YandexMarket);
		public MarketplaceProductModel AmazonUkMarketPlaceProduct =>
			MarketplaceProducts?.FirstOrDefault(prod => prod.MarketplaceId ==
			(int)Marketplace.AmazonUK);
		public MarketplaceProductModel OzMarketPlaceProduct =>
			MarketplaceProducts?.FirstOrDefault(prod => prod.MarketplaceId == (int)Marketplace.Ozon);

		public IEnumerable<ProductImageModel> ProductImages { get; set; }

		public IEnumerable<ProductImageModel> SmallGalleryImages =>
			 ProductImages?.Where(img => img.ImageType == Enums.ProductImageType.SmallGallery)
			.OrderBy(img => img.Order);

		public IEnumerable<ProductImageModel> BigGalleryImages =>
			 ProductImages?.Where(img => img.ImageType == Enums.ProductImageType.BigGallery)
			.OrderBy(img => img.Order);

		[Display(Name = "Описание")]
		public string Description { get; set; }
		[Display(Name = "Описание для бота")]
		public string BotDescription { get; set; }

		[Display(Name = "Длинное описание")]
		public string LongDescription { get; set; }

		[Display(Name = "Информация")]
		public string Information { get; set; }
		[Display(Name = "Характеристики для бота")]
		public string BotInformation { get; set; }

		[Display(Name = "Отзывы")]
		public string ReviewsText { get; set; }

		public string CoverCatalogImage => ProductImages.FirstOrDefault(image =>
		image.ImageType == Enums.ProductImageType.Catalog)?.FileName;

		public string MobileCoverCatalogImage => ProductImages.FirstOrDefault(image =>
		image.ImageType == Enums.ProductImageType.MobileCatalog)?.FileName;

		public string CoverCatalogImageFullPath => !string.IsNullOrWhiteSpace(CoverCatalogImage)
				? $"{BucketUrl}/{CoverCatalogImage}"
				: string.Empty;
	
		public string MobileCoverCatalogImageFullPath => !string.IsNullOrWhiteSpace(MobileCoverCatalogImage)
			? $"{BucketUrl}/{MobileCoverCatalogImage}"
			: string.Empty;

		public string CountryPrefix { get; set; }
		public string ISBN { get; set; }
	}
}
