using ICan.DomainModel.Jobs;
using ICan.DomainModel.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
#pragma warning disable IDE0005
using OpenQA.Selenium.Remote;
#pragma warning restore IDE0005
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ICan.Jobs.WB
{
	public class CheckWarehouseJob
	{
		class AlertList : List<(DateTime date, string city, IEnumerable<string> names)> { }

		private const string EmailName = "email_or_login";
		private const string PasswordName = "password";
		private const string SubmitLoginName = "login-form_button";

		private const string DatePickerXPath = "//div[@class='DayPickerInput']//input";
		private const string CitiesSelectXPath = "//div[@class='selectButtonContainer']//button";
		private const string CityElementListXPath = "//li[@class='selectDropdownContainerItem']//span[normalize-space(text()) = '{0}']";
		private const string RowXPath = "//div[@class='Calendar-container__supplies']//div[starts-with(@class,'Supply-group__days')]//div[starts-with(@class,'Supply-group__day')][position() >= {0}]//div[starts-with(@class,'Supply-group__limit-data')]";

		private const string CellXPath = "//div[@class='Calendar-container__supplies']//div[starts-with(@class,'Supply-group__days')]//div[starts-with(@class,'Supply-group__day')][position() = {0}]//div[starts-with(@class,'Supply-group__limit-data')]";

		private const string SuperSafeRowXPath = "//div[@class='Calendar-container__supplies']//div[starts-with(@class,'Supply-group__days')]//div[starts-with(@class,'Supply-group__day')][position() >= 1]//div[starts-with(@class,'Supply-group__limit-data')][6]";
		//Wrapped-turnover-revenue__day-wrapper
		private const string RevenueItemXPath = "//div[starts-with(@class,'Wrapped-turnover-revenue__day-wrapper')]//div[@class='Wrapped-turnover-revenue__day']//span";
		private const string RevenuePalletXPath = "//div[starts-with(@class,'Wrapped-turnover-revenue__pallet-wrapper')]//div[@class='Wrapped-turnover-revenue__day']//span";
		private const string RevenueNameLimitXPath =
			"//div[starts-with(@class,'Wrapped-turnover-revenue__table')]//tbody//tr//td[1]//span[text()='{0}']/ancestor::tr";
		private const string RevenueMonoLimitXPath = "//td[2]//span";
		private const string RevenueMixLimitXPath = "//td[3]//span";
		private const string RevenueSuperSafeLimitXPath = "//td[4]//span";
		private const string RevenuePaletteLimitXPath = "//td[5]//span";


		private const string MonoCellsXPath = RowXPath + "[3]";
		private const string MixCellsXPath = RowXPath + "[4]";
		private const string MonoPalletCellsXPath = RowXPath + "[5]";

		private const string MonoSingleCellsPath = CellXPath + "[3]";
		private const string MixSingleCellXPath = CellXPath + "[4]";
		private const string MonoPalletSingleCellXPath = CellXPath + "[5]";
		private readonly string _appURL;
		private readonly IWebDriver _driver;
		private readonly string _login;
		private readonly string _password;
		private readonly string _warehouseURL;
		private readonly string _turnoverURL;
		private readonly string _phones;
		private readonly string _exchange;
#pragma warning disable IDE0005
		private readonly string _serviceUrl;
#pragma warning restore IDE0005
		private readonly string _browserlessapiKey;
		private readonly string _chromeDriverDir;
		private int? _revenueItemValue;
		private int? _revenuePalleteValue;
		private readonly IModel _rbmqModel;
		private readonly ILogger<CheckWarehouseJob> _logger;
		private List<WbCityStat> _citiesStats;

		public static readonly int WeekCount = 5;
		private readonly IEnumerable<WbCitySetting> _enabledCities;
		private IEnumerable<string> _enabledCitiesNames => _enabledCities?.Select(city => city.Name);
		private readonly int _wareHouseLimit;
		private readonly string _checkMode;

		public CheckWarehouseJob(IConfiguration configuration, IModel rbmqModel, ILogger<CheckWarehouseJob> logger, WbCheckWarehouseSetting wbsetting)
		{
			_appURL = configuration["Settings:WB:MainPageUrl"];
			_warehouseURL = configuration["Settings:WB:WarehouseUrl"];
			_turnoverURL = configuration["Settings:WB:TurnoverUrl"];
		
			_serviceUrl = configuration["Settings:Jobs:ServiceUrl"];
			_browserlessapiKey = configuration["Settings:Jobs:BrowserlessApiKey"];
			_chromeDriverDir = configuration["Settings:Jobs:ChromeDriverDir"];
			_driver = GetDriver();

			_login = configuration["Settings:WB:UserLogin"];
			_password = configuration["Settings:WB:UserPassword"];
			_phones = configuration["Settings:SMS:Phones"];
			_exchange = wbsetting.Exchange;

			var citiesSetting = configuration["Settings:Jobs:WbCheckWarehouse:Cities"];
			_enabledCities = wbsetting.Cities.Where(city => city.Enabled);
			_wareHouseLimit = wbsetting.Limit;
			_checkMode = wbsetting.Mode;
			_rbmqModel = rbmqModel;
			_logger = logger;
			InitCitiesStats();
		}
 
		private void InitCitiesStats()
		{
			_citiesStats = _enabledCities?.Select(city =>
			   new WbCityStat
			   {
				   Name = city.Name,
				   Days = city.DaysGap,
				   ConsiderMono = city.ConsiderMono,
				   ConsiderMonoPallet = city.ConsiderMonoPallet
			   }).ToList();
		}

		private IWebDriver GetDriver()
		{
#if DEBUG
			try
			{
				var chromeDriverDir = !string.IsNullOrWhiteSpace(_chromeDriverDir)
					? _chromeDriverDir
					: AppDomain.CurrentDomain.BaseDirectory;

				return new ChromeDriver(chromeDriverDir);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "error in getting driver");
				throw;
			}
#else
			var chromeOptions = GetOptions();
			return new RemoteWebDriver(new Uri(_serviceUrl), chromeOptions.ToCapabilities(), TimeSpan.FromMinutes(5));
#endif
		}

#pragma warning disable IDE0051
		private ChromeOptions GetOptions()
#pragma warning restore IDE0051
		{
			ChromeOptions chromeOptions = new ChromeOptions();
			// Set launch args similar to puppeteer's for best performance
			chromeOptions.AddArgument("--disable-background-timer-throttling");
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
			chromeOptions.AddAdditionalCapability("browserless.token", _browserlessapiKey, true);
			return chromeOptions;
		}

		public bool CheckWarehouse()
		{
			Authorize();
			GatherStats();
			Check();
			_driver.Quit();
			return true;
		}

		private void GatherStats()
		{
			_driver.Navigate().GoToUrl(_turnoverURL);
			Thread.Sleep(3000);
			_revenueItemValue = GetRevenue(RevenueItemXPath);
			_revenuePalleteValue = GetRevenue(RevenuePalletXPath);
			GetWhLimits();
		}

		private void GetWhLimits()
		{
			try
			{
				foreach (var city in _citiesStats)
				{
					var limitXPath = string.Format(RevenueNameLimitXPath, city.Name);
					city.MonoLimit = GetLimitValue(string.Concat(limitXPath, RevenueMonoLimitXPath));
					city.MixLimit = GetLimitValue(string.Concat(limitXPath, RevenueMixLimitXPath));
					city.SupersafeLimit = GetLimitValue(string.Concat(limitXPath, RevenueSuperSafeLimitXPath));
					city.MonopaletteLimit = GetLimitValue(string.Concat(limitXPath, RevenuePaletteLimitXPath));
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "stat limits");
				throw;
			}
		}

		private int? GetLimitValue(string xPath)
		{
			try
			{
				var elm = _driver.FindElement(By.XPath(xPath));
				return int.TryParse(elm.Text, out var value) ? value : (int?)null;
			}
			catch (NoSuchElementException ex)
			{
				return null;
			}
		}

		private int? GetRevenue(string xpath)
		{
			var revenueSpan = _driver.FindElement(By.XPath(xpath));
			var revenueText = revenueSpan?.Text;
			if (string.IsNullOrEmpty(revenueText))
				return null;
			if (int.TryParse(revenueText.Substring(0, revenueText.IndexOf(" руб")), out var parsedValue))
			{
				return parsedValue;
			}
			return null;
		}

		private void Check()
		{
			_driver.Navigate().GoToUrl(_warehouseURL);
			Thread.Sleep(800);
			DateTime currentDate = DateTime.MinValue, startDate = DateTime.MinValue;
			AlertList alertList = new AlertList();
			_logger.LogWarning($"[Job][CheckWhWb] start limit { _wareHouseLimit} cities {string.Join(", ", _enabledCitiesNames)}");

			int week = 0;
			int cityErr = 0;
			foreach (var city in _enabledCities)
			{
				try
				{
					SetCity(city.Name);
					if (city.Name.Equals(_enabledCities.First().Name))
					{
						var actions = new Actions(_driver);
						actions.MoveToElement(_driver.FindElement(By.XPath(SuperSafeRowXPath)));
						actions.Perform();
					}
					var whLimit = _citiesStats.FirstOrDefault(stat =>
					 stat.Name.Equals(city.Name,StringComparison.InvariantCultureIgnoreCase));
					//Dates or DaysGap
					if (string.IsNullOrWhiteSpace(_checkMode) || _checkMode.Equals("DaysGap", StringComparison.OrdinalIgnoreCase))
					{
						week = CheckWeeks(ref currentDate, alertList, city, whLimit);
					}
					else
					{
						CheckDates(city, alertList, whLimit);
					}

				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"[Job][CheckWhWb] outer {city} {week} {currentDate}");
					cityErr++;
					if (cityErr > 3)
						break;
				}
			}
			try
			{
				PublishToQueue(alertList);
				_logger.LogWarning($"[Job][CheckWhWb] after publish  {DateTime.Now.ToShortDateString()}");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"[Job][CheckWhWb] Publish to queue failed");
			}
		}

		private void CheckDates(WbCitySetting city, AlertList alertList, WbCityStat whLimit)
		{
			var currentDate = DateTime.MinValue;
			try
			{
				foreach (var rawDate in city.Dates)
				{
					currentDate = DateTime.Parse(rawDate);
					SetDate(currentDate);
					var check = NeedDateAlert(city, whLimit, currentDate);
					List<string> names = AddToAlert(currentDate, alertList, city, check);
					_logger.LogWarning($"[Job][CheckWhWb] #done {city} {currentDate} need alert {check.NeedAlert} {string.Join(",", names)}");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"[Job][ChecDateskWhWb] {city} {currentDate}");
			}
		}

		private AlertResult NeedDateAlert(WbCitySetting city, WbCityStat whLimit, DateTime currentDate)
		{
			var position = (currentDate.DayOfWeek == DayOfWeek.Sunday) ? 7 : (int)currentDate.DayOfWeek;
			var result = new AlertResult();
			if (city.ConsiderMono)
			{
				var path = string.Format(MonoSingleCellsPath, position);
				var monocell = _driver.FindElement(By.XPath(path));
				var text = monocell.Text;
				result.Mono = (int.TryParse(text, out var value) && value > _wareHouseLimit);
				if (whLimit?.MonoLimit != null && result.Mono)
				{
					result.Mono = result.Mono && (whLimit.MonoLimit.Value <= _revenueItemValue);
				}
				_logger.LogWarning($"[Job][ChecDateskWhWb] mono cell {city.Name} {currentDate} {text} {whLimit?.MonoLimit} {result.Mono} {path}");
			}
			if (city.ConsiderMonoPallet)
			{
				var path = string.Format(MonoPalletCellsXPath, position);
				var monoPalletecell = _driver.FindElement(By.XPath(path));
				var text = monoPalletecell.Text;
				result.MonoPallette =  (int.TryParse(text, out var value) && value > _wareHouseLimit);
				if (whLimit?.MonopaletteLimit != null && result.MonoPallette)
				{
					result.MonoPallette = result.MonoPallette && (whLimit.MonopaletteLimit.Value <= _revenuePalleteValue);
				}
				_logger.LogWarning($"[Job][ChecDateskWhWb] monopallete cell {city.Name} {currentDate} {text} {whLimit?.MonoLimit} {result.Mono} {path}");
			}
			return result;
		}

		private int CheckWeeks(ref DateTime currentDate, AlertList alertList, WbCitySetting city, WbCityStat whLimit)
		{
			var startDate = GetStartDate(city.Name);
			currentDate = startDate;
			int week;
			int weekRetry = 0;

			for (week = 0; week < WeekCount; week++)
			{
				try
				{
					currentDate = startDate.AddDays(week * 7);
					SetDate(currentDate);
					var check = NeedAlert(week, currentDate, whLimit);
					List<string> names = AddToAlert(currentDate, alertList, city, check);
					_logger.LogWarning($"[Job][CheckWhWb] #done {city} {week} {currentDate} need alert {check.NeedAlert} {string.Join(",", names)}");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"[Job][CheckWhWb] inner {city} {week} {currentDate}");
					week--;
					weekRetry++;
					if (weekRetry > 3)
						break;
				}
			}

			return week;
		}

		private static List<string> AddToAlert(DateTime currentDate, AlertList alertList, WbCitySetting city, AlertResult check)
		{
			var names = new List<string>();
			if (check.NeedAlert)
			{
				if (check.Mono)
				{
					names.Add("Моно");
				}
				if (check.MonoPallette)
				{
					names.Add("МоноПаллет");
				}
				alertList.Add((currentDate, city.Name, names));
			}

			return names;
		}

		private void SetDate(DateTime currentDate)
		{
			var dateElm = _driver.FindElement(By.XPath(DatePickerXPath));
			dateElm.Click();
			Enumerable.Range(0, 10).ToList().ForEach(
				 elm => dateElm.SendKeys(Keys.Backspace));
			dateElm = _driver.FindElement(By.XPath(DatePickerXPath));
			dateElm.SendKeys(currentDate.ToShortDateString());
			Thread.Sleep(700);
		}

		private void SetCity(string city)
		{
			_driver.FindElement(By.XPath(CitiesSelectXPath)).Click();
			Thread.Sleep(800);
			var cityStr = string.Format(CityElementListXPath, city);
			_driver.FindElement(By.XPath(cityStr)).Click();
			Thread.Sleep(700);
		}

		private DateTime GetStartDate(string city)
		{
			var date = DateTime.Today;
			int days = _citiesStats.First(stat => stat.Name.Equals(city)).Days;
			return date.AddDays(days);
		}

		private void PublishToQueue(AlertList alertList)
		{
			if (alertList == null || !alertList.Any())
				return;
			var smsText = string.Join("; ", alertList.Select(item => $"{item.city}, {item.date.ToString("dd.MM")} {string.Join(", ", item.names)}"));
			var message = new SmsMessage
			{
				Text = smsText,
				PhoneNumbers = _phones
			};

			var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
			_rbmqModel.BasicPublish(exchange: _exchange,
								 routingKey: "",
								 basicProperties: null,
								 body: body);
			_logger.LogWarning($"[Job][CheckWhWb] message published tezt {message.Text} phones {message.PhoneNumbers} {DateTime.Now.ToShortDateString()}");
		}

		private AlertResult NeedAlert(int week, DateTime currentDate, WbCityStat whLimit)
		{
			var thisWeekDay = (currentDate.DayOfWeek == DayOfWeek.Sunday) ? 7 : (int)currentDate.DayOfWeek;
			var position = week == 0 ? thisWeekDay : 1;
			var monocells = _driver.FindElements(By.XPath(string.Format(MonoCellsXPath, position)))
				.ToList();
			var result = new AlertResult();

			result.Mono = monocells.Any(elm => (int.TryParse(elm.Text, out var value) && value > _wareHouseLimit));
			if (whLimit?.MonoLimit != null && result.Mono && whLimit.ConsiderMono)
			{
				result.Mono = result.Mono && (whLimit.MonoLimit.Value <= _revenueItemValue);
			}

			var monoPalletecells = _driver.FindElements(By.XPath(string.Format(MonoPalletCellsXPath, position)));
			result.MonoPallette = monoPalletecells.Any(elm => (int.TryParse(elm.Text, out var value) && value > _wareHouseLimit));
			if (whLimit?.MonopaletteLimit != null && result.MonoPallette && whLimit.ConsiderMonoPallet)
			{
				result.MonoPallette = result.MonoPallette && (whLimit.MonopaletteLimit.Value <= _revenuePalleteValue);
			}

			return result;
		}

		private void Authorize()
		{
			_driver.Navigate().GoToUrl(_appURL);
			_driver.FindElement(By.Name(EmailName)).SendKeys(_login);
			Thread.Sleep(700);
			_driver.FindElement(By.Name(PasswordName)).SendKeys(_password);
			Thread.Sleep(700);
			_driver.FindElement(By.ClassName(SubmitLoginName)).Click();
			Thread.Sleep(700);
		}
	}
}
