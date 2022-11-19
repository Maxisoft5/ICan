namespace ICan.Common.Models.Opt
{
	public class UkMarketplaceProductModel : MarketplaceProductModel
	{
		public override string ReviewsAmountDescr
				=> (!ReviewsAmount.HasValue || ReviewsAmount == 0 || ReviewsAmount > 1) ? "reviews" : "review";

		public override string DisplayСurrency => $"£";
		public override string DisplayPrice => Price?.ToString("N2");
		public override string DisplayPriceAndCurrency => $"{DisplayСurrency} {DisplayPrice}";
	}
}
