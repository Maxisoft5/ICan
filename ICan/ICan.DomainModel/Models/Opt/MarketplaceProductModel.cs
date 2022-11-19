using ICan.Common.Utils;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ICan.Common.Models.Opt
{
	public class MarketplaceProductModel
	{
		public int MarketplaceProductId { get; set; }

		public int ProductId { get; set; }

		[Display(Name = "Маркетплейс")]
		public int MarketplaceId { get; set; }

		public string MarketplaceName { get; set; }
		
		public string AllUrls { get; set; }
		[Display(Name = "Ссылки")]
		public IEnumerable<MarketplaceProductUrlModel> Urls { get; set; }

		[Display(Name = "Код")]
		public string Code { get; set; }

		[Display(Name = "Цена")]
		public decimal? Price { get; set; }

		[Display(Name = "Рейтинг")]
		public float? Raiting { get; set; }

		[Display(Name = "Количество отзывов")]
		public int? ReviewsAmount { get; set; }

		[Display(Name = "Ссылка")]
		public string Url => Urls.FirstOrDefault()?.Url;

		[Display(Name = "Показывать на сайте")]
		public bool ShowOnSite { get; set; }

		public virtual string ReviewsAmountDescr => Util.GetreviewsDescr(ReviewsAmount);

		public virtual string DisplayPrice => Price?.ToString("N0");
		public virtual string DisplayСurrency => $"₽";
		public virtual string DisplayPriceAndCurrency => $"{DisplayPrice} {DisplayСurrency}";
	}
}
