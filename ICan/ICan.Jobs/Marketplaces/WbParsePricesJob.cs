using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
#pragma warning disable IDE0005
using OpenQA.Selenium.Remote;
#pragma warning restore IDE0005
using System;

namespace ICan.Jobs.Marketplaces
{
	public class WbParsePricesJob : ParsePricesJob
	{
		protected string _priceWithoutCommissionPath = "";
		public WbParsePricesJob(
			IConfiguration configuration,
			ISiteRepository siteRepository,
			ILogger<ParsePricesJob> logger) : base(configuration, siteRepository, logger, Marketplace.WB, 60000)
		{
			_pricePath = configuration["Settings:Jobs:WbParsePrices:PricePath"];
			_priceWithoutCommissionPath = configuration["Settings:Jobs:WbParsePrices:PriceWithoutComissionPath"];
			_reviewsAmountPath = configuration["Settings:Jobs:WbParsePrices:ReviewsAmountPath"];
			_ratingPath = configuration["Settings:Jobs:WbParsePrices:RatingPath"];
		}

		protected override int? GetReviewsAmount(IWebDriver driver, OptMarketplaceProduct notebook)
		{
			try
			{
				var reviewsElement = driver.FindElement(By.XPath(_reviewsAmountPath));

				if (reviewsElement == null)
				{
					_logger.LogWarning($"[MarketplaceProduct][ReviewsAmount] no reviews element{notebook.Product.Name} MarketplaceProductId  {notebook.MarketplaceProductId}");
					return null;
				}
				var reviewsElementText = reviewsElement.Text;
				var text = reviewsElementText.Substring(0, reviewsElementText.IndexOf("îòçû") - 1);
				if (int.TryParse(text, out var reviewsAmount))
					return reviewsAmount;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"[MarketplaceProduct][ReviewsAmount] {notebook.Product.Name} MarketplaceProductId {notebook.MarketplaceProductId}");
			}
			return null;
		}

		protected override float? GetRating(IWebDriver driver, OptMarketplaceProduct notebook)
		{
			try
			{
				var ratingElmText = driver.FindElement(By.XPath(_ratingPath)).Text;

				if (float.TryParse(ratingElmText, out var rating))
					return rating;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"[MarketplaceProduct][Rating] {notebook.Product.Name} MarketplaceProductId  {notebook.MarketplaceProductId}");
			}
			return null;
		}

		protected override decimal? GetPrice(IWebDriver driver, OptMarketplaceProduct notebook)
		{
			var element = driver.FindElement(By.TagName("html"));
			string body = string.Empty;
			IJavaScriptExecutor js = driver as IJavaScriptExecutor;
			if (js != null)
			{
				body = (string)js.ExecuteScript("return arguments[0].innerHTML;", element);
			}
			var message = $"{notebook.Product.Name}, price path {_pricePath}, price without  comission: {_priceWithoutCommissionPath}";
			try
			{
				//Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();
				//message += $" pic {ss.AsBase64EncodedString}";
				var priceWithoutComission = GetPriceWithoutComission(driver);
				if (priceWithoutComission.HasValue)
				{
					_logger.LogWarning($"[MarketplaceProduct][Price][WithCommision] {priceWithoutComission}");
					return priceWithoutComission;
				}
				var priceElmText = driver.FindElement(By.XPath(_pricePath)).GetAttribute("textContent").Trim();
				message += $" priceElmText  {priceElmText}";

				var text = priceElmText.Substring(0, priceElmText.LastIndexOf(" ")).Replace(" ", "");
				if (decimal.TryParse(text, out var price))
				{
					_logger.LogWarning($"[MarketplaceProduct][Price] {message}");
					return price;
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning($"[MarketplaceProduct][Price][Error] {message}");
				_logger.LogError(ex, $"[MarketplaceProduct][Price] {notebook.Product.Name} MarketplaceProductId  {notebook.MarketplaceProductId}  body {body}");
			}
			_logger.LogWarning($"[MarketplaceProduct][Price][Final] {message}");
			return null;
		}

		private decimal? GetPriceWithoutComission(IWebDriver driver)
		{
			try
			{
				var priceElm = driver.FindElement(By.XPath(_priceWithoutCommissionPath));
				if (priceElm != null)
				{
					var priceElmText = priceElm.GetAttribute("textContent").Trim();
					var text = priceElmText.Substring(0, priceElmText.LastIndexOf(" ")).Replace(" ", "");
					if (decimal.TryParse(text, out var price))
					{
						_logger.LogWarning($"[MarketplaceProduct][PriceWithoutComission] priceElmText  {priceElmText}");
						return price;
					}
				}
			}
			catch (NoSuchElementException)
			{

			}
			return null;
		}
	}
}
