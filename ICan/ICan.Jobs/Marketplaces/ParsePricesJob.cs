using ICan.Common.Domain;
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
using System.Linq;
using System.Threading;

namespace ICan.Jobs.Marketplaces
{
	public abstract class ParsePricesJob
	{
		protected readonly IWebDriver _driver;
		protected readonly ISiteRepository _siteRepository;
		protected readonly ILogger<ParsePricesJob> _logger;


		protected readonly int _marketPlaceId;
		private readonly int _timeout;
		protected readonly IConfiguration _configuration;
		protected string _pricePath = "";
		protected string _reviewsAmountPath = "";
		protected string _ratingPath = "";

		public ParsePricesJob(IConfiguration configuration,
			ISiteRepository siteRepository,
			ILogger<ParsePricesJob> logger,
			Marketplace marketplace,
			int timeout)
		{
			_siteRepository = siteRepository;
			_logger = logger;
			_marketPlaceId = (int)marketplace;
			_configuration = configuration;
			_timeout = timeout;
		}

		protected IWebDriver GetDriver(string chromeDriverDir, string serviceUrl, string browserlessapiKey)
		{
#if DEBUG
			try
			{
				chromeDriverDir = !string.IsNullOrWhiteSpace(chromeDriverDir)
				  ? chromeDriverDir
				  : AppDomain.CurrentDomain.BaseDirectory;
				var chromeOptions = GetOptions("");
				return new ChromeDriver(chromeDriverDir, chromeOptions);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "error in getting driver");
				throw;
			}
#else
			_logger.LogInformation("brwoserless chrome driver");
			var chromeOptions = GetOptions(browserlessapiKey);
			return new RemoteWebDriver(new Uri(serviceUrl), chromeOptions.ToCapabilities(), TimeSpan.FromMinutes(5));
#endif
		}

#pragma warning disable IDE0051
		private ChromeOptions GetOptions(string browserlessApiKey)
#pragma warning restore IDE0051
		{
			var chromeOptions = new ChromeOptions();
			if (string.IsNullOrWhiteSpace(browserlessApiKey))
				return chromeOptions;
			//// Set launch args similar to puppeteer's for best performance
			chromeOptions.AddArgument("--disable-background-timer-throttling");
			chromeOptions.AddArgument("--disable-gpu");
			chromeOptions.AddArgument("--disable-backgrounding-occluded-windows");
			chromeOptions.AddArgument("--disable-breakpad");
			chromeOptions.AddArgument("--disable-component-extensions-with-background-pages");
			chromeOptions.AddArgument("--disable-dev-shm-usage");
			chromeOptions.AddArgument("--disable-extensions");
			chromeOptions.AddArgument("--disable-features=TranslateUI,BlinkGenPropertyTrees");
			chromeOptions.AddArgument("--disable-ipc-flooding-protection");
			chromeOptions.AddArgument("--disable-renderer-backgrounding");
			chromeOptions.AddArgument("--enable-features=NetworkService,NetworkServiceInProcess");
			chromeOptions.AddArgument("--force-color-profile=srgb");
			chromeOptions.AddArgument("--hide-scrollbars");
			chromeOptions.AddArgument("--metrics-recording-only");
			chromeOptions.AddArgument("--mute-audio");
			chromeOptions.AddArgument("--headless");
			chromeOptions.AddArgument("--no-sandbox");
			chromeOptions.AddArgument("--window-size=1920,1080");
			chromeOptions.AddAdditionalCapability("browserless.token", browserlessApiKey, true);
			return chromeOptions;
		}

		public virtual bool GetMarketInfo()
		{
			var notebooks = _siteRepository.GetMarketplaceProducts(_marketPlaceId);
			foreach (var notebook in notebooks)
			{
				if (notebook.Urls == null || !notebook.Urls.Any())
				{
					_logger.LogWarning($"[MarketplaceProduct][NoUrl] marketPlace {_marketPlaceId} {notebook.Product.Name} CountryId {notebook.Product.CountryId}");
					continue;
				}
				var productUrl = string.Empty;
				try
				{
					Thread.Sleep(_timeout); 	
					_logger.LogWarning($"[MarketplaceProduct][Start] marketPlace{_marketPlaceId} {notebook.Product.Name}");
					var browserlessapiKey = _configuration["Settings:Jobs:BrowserlessApiKey"];
					var serviceUrl = _configuration["Settings:Jobs:ServiceUrl"];
					var chromeDriverDir = _configuration["Settings:Jobs:ChromeDriverDir"];

					var driver = GetDriver(chromeDriverDir, serviceUrl, browserlessapiKey);
					
					productUrl = notebook.Urls.First().Url;
					driver.Navigate().GoToUrl(productUrl);
					Thread.Sleep(_timeout);
					var price = GetPrice(driver, notebook) ?? notebook.Price;
					var rating = GetRating(driver, notebook) ?? notebook.Raiting;
					var reviews = GetReviewsAmount(driver, notebook) ?? notebook.ReviewsAmount;

					driver.Quit();

					_siteRepository.UpdateAsync(_marketPlaceId, notebook.ProductId, price, reviews, rating).GetAwaiter().GetResult();
					_logger.LogWarning($"[MarketplaceProduct][Finish] marketPlace {_marketPlaceId} {notebook.Product.Name}");
				}
				catch (Exception ex)
				{
					_logger.LogWarning($"[MarketplaceProduct][Error] marketPlace {_marketPlaceId} {notebook.Product.Name}");
					_logger.LogError(ex, $"[MarketplaceProduct] {notebook.Product.Name} {productUrl}");
				}
			}
			return true;
		}

		protected virtual int? GetReviewsAmount(IWebDriver driver, OptMarketplaceProduct notebook)
		{
			return null;
		}

		protected virtual float? GetRating(IWebDriver driver, OptMarketplaceProduct notebook)
		{
			return null;
		}

		protected virtual decimal? GetPrice(IWebDriver driver, OptMarketplaceProduct notebook)
		{
			return null;
		}
	}
}
