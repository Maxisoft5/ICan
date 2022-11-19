using ICan.Common.Domain;
using ICan.Common.Jobs;
using ICan.Common.Models.Enums;
using ICan.Common.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
#pragma warning disable IDE0005
using OpenQA.Selenium.Remote;
#pragma warning restore IDE0005
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ICan.Jobs.Marketplaces
{
	public class OzonParsePricesJob : ParsePricesJob
	{
		private static readonly string _commonPageUrl = "https://www.ozon.ru/publisher/ya-mogu-70816609/";
		private static readonly string _reviewsPath = "//a[contains(@href,\"#comments\")]";

		public OzonParsePricesJob(IConfiguration configuration,
			ISiteRepository siteRepository,
			ILogger<ParsePricesJob> logger) : base(configuration, siteRepository, logger, Marketplace.Ozon, 1800000)
		{
		}

		public override bool GetMarketInfo()
		{
			var notebooks = _siteRepository.GetMarketplaceProducts(_marketPlaceId);

			try
			{
				_logger.LogWarning($"[MarketplaceProduct][Start] marketPlace {_marketPlaceId}");
				var browserlessapiKey = _configuration["Settings:Jobs:BrowserlessApiKey"];
				var serviceUrl = _configuration["Settings:Jobs:ServiceUrl"];
				var chromeDriverDir = _configuration["Settings:Jobs:ChromeDriverDir"];
				var driver = GetDriver(chromeDriverDir, serviceUrl, browserlessapiKey);
				driver.Navigate().GoToUrl(_commonPageUrl);
				Thread.Sleep(60000);
				IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
				js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
				Thread.Sleep(60000);
				var reviews = GetAllReviews(driver);
				driver.Quit();
				foreach (var notebook in notebooks)
				{
					if (string.IsNullOrWhiteSpace(notebook.Code))
					{
						_logger.LogWarning($"[MarketplaceProduct][NoUrl] marketPlace {_marketPlaceId} {notebook.Product.Name} CountryId {notebook.Product.CountryId}");
						continue;
					}
					notebook.ReviewsAmount = reviews.FirstOrDefault(review => string.Equals(review.Code, notebook.Code, StringComparison.InvariantCultureIgnoreCase))?.ReviewsAmount ?? notebook.ReviewsAmount;

					_siteRepository.UpdateMarketPlaceProductAsync(notebook).GetAwaiter().GetResult();
					_logger.LogWarning($"[MarketplaceProduct][Finish] marketPlace{_marketPlaceId} {notebook.Product.Name}");
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning($"[MarketplaceProduct][Error] marketPlace {_marketPlaceId}");
				_logger.LogError(ex, $"[MarketplaceProduct] marketPlace {_marketPlaceId}");
			}
			return true;
		}

		private IEnumerable<OzonReview> GetAllReviews(IWebDriver driver)
		{
			var list = new List<OzonReview>();
			try
			{
				var template = "https://www.ozon.ru/product/";
				var reviews = driver.FindElements(By.XPath(_reviewsPath))?.ToArray();
				for(int i = 0; i < reviews.Count(); i++)
				{
					var review = reviews[i];
					try
					{
						var text = review.Text;
						var reviewCount = text.Substring(0, text.IndexOf(" "));
						var href = review.GetAttribute("href");
						var code = href.Substring(href.IndexOf(template) + template.Length, href.IndexOf("/?") - template.Length);
						var currentRatePath = string.Format($"({_reviewsPath})[{{0}}]//preceding-sibling::div//div[contains(@style,'width')]", i+1);
						var rateElm = review.FindElement(By.XPath(currentRatePath));
						var styleWidth = rateElm.GetAttribute("style").Split(":")[1];
						var width = styleWidth.Substring(0, styleWidth.Length - 2)
							.Replace(".", ",");
						
						var rate = (float?)null;
						if (float.TryParse(width, out var parsedRate))
						{
							rate = (float)Math.Round(parsedRate / 20f, 2);
						}
						 
						list.Add(new OzonReview
						{
							Url = text,
							Code = code,
							ReviewsAmount = int.TryParse(reviewCount, out var parsed) ? parsed : null,
							Rate = rate
						});
					}
					catch (Exception ex)
					{
						_logger.LogError($"[Ozon][SingleReview]{review} {ex}");
					}

				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"[Ozon][Reviews]  {ex}");
			}
			return list;
		}
	}
}
