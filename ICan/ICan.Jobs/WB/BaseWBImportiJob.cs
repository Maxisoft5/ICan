using ICan.Business.Managers;
using ICan.Business.Services;
using ICan.Common.Domain;
using ICan.Common.Models.Opt;
using ICan.Data.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ICan.Jobs.WB
{
	public class BaseWBImportiJob<TDb> where TDb : class, IWBItemWithProductId
	{
		private readonly IEmailSender _emailSender;
		private static string _datesFormat = "yyyy-MM-ddTHH:mm:ss";

		private readonly string _apiKey;

		private static readonly HttpClient HttpClient = new HttpClient();

		private readonly ProductManager _productManager;
		private readonly List<string> Emails;
		private readonly ApplicationDbContext _context;

		public int DaysInScope { get; }
		public IEnumerable<string> ArticlesToIgnore { get; }
		public int LongPeriodDaysInScope { get; }
		public DateTime StartingDate { get; }

		protected string Url;
		protected string DataTypeStr;
		protected readonly ILogger _logger;

		public BaseWBImportiJob(ILogger<BaseWBImportiJob<TDb>> logger, IConfiguration configuration, IEmailSender emailSender, ApplicationDbContext context, ProductManager productManager)
		{
			_logger = logger;
			_emailSender = emailSender;
			_apiKey = configuration["Settings:WB:ApiKey"];
			var rawEmails = configuration["Settings:WB:ReportEmails"];
			Emails = (string.IsNullOrWhiteSpace(rawEmails) ? Enumerable.Empty<string>()
				: rawEmails.Split(",")).ToList();
			_context = context;
			DaysInScope = int.Parse(configuration["Settings:WB:DaysInScope"]);

			LongPeriodDaysInScope = int.Parse(configuration["Settings:WB:LongPeriodDaysInScope"]);
			StartingDate = DateTime.ParseExact(configuration["Settings:WB:StartingDate"], "dd.MM.yyyy", null);
			_productManager = productManager;

			var rawArticles = configuration["Settings:WB:ArticlesToIgnore"];

			ArticlesToIgnore = string.IsNullOrWhiteSpace(rawArticles) ? Enumerable.Empty<string>() : rawArticles.Split(",");
		}

		public bool Import()
		{
			_logger.LogInformation($"[Job][{typeof(TDb).Name}] started");
			var maxDate = DateTime.Now.AddDays(-1);
			var minDate = maxDate.AddDays(-DaysInScope);
			bool result = ImportInternal(maxDate, minDate);
			return result;
		}

		public bool ImportSeveralMonths()
		{
			_logger.LogInformation($"[Job][{typeof(TDb).Name}][Long] started");
			var maxDate = DateTime.Now.AddDays(-1);
			var minDate = maxDate.AddDays(-LongPeriodDaysInScope);
			minDate = minDate > StartingDate ? minDate : StartingDate;
			bool result = ImportInternal(maxDate, minDate);
			return result;
		}

		public bool ImportCustom(DateTime minDate)
		{
			_logger.LogInformation($"[Job][{typeof(TDb).Name}][Custom] started");
			var maxDate = DateTime.Now.AddDays(-1);
			bool result = ImportInternal(maxDate, minDate);
			return result;
		}

		private bool ImportInternal(DateTime maxDate, DateTime minDate)
		{
			var result = true;
			try
			{
				var date = minDate.Date;
				maxDate = maxDate.Date;
				var productsWithIds = AsyncContext.Run(() => _productManager.GetProductsWithIsbn());
				for (int i = 0; date < maxDate; i++)
				{
					date = minDate.AddDays(i);
					AsyncContext.Run(() => Import(Url, date, productsWithIds));
					_logger.LogWarning($"[Job][{typeof(TDb).Name}][Custom] {date}");
					Thread.Sleep(60000);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,
					$"[Wb][API][{DataTypeStr}] Возникла ошибка при импорте");
				result = false;
			}
			if (!result)
			{
				SendErrorMessage();
			}

			return result;
		}

		protected void SendErrorMessage()
		{
			Emails.ForEach(email =>
			{
				_emailSender.SendEmail(email, $"[{DataTypeStr}] При загрузке данных из WB возникла ошибка", string.Empty);
			});
		}


		public async Task<IEnumerable<TDb>> Import(string urlTemplate, DateTime? date = null, Dictionary<int, IdIsbnNameProductModel> productsWithIds = null)
		{
			try
			{
				productsWithIds = productsWithIds ?? (await _productManager.GetProductsWithIsbn());
				var currentDate = date ?? DateTime.Now.Date;
				var dateBeforeFrom = currentDate.ToString(_datesFormat);

				var url = string.Format(urlTemplate, dateBeforeFrom, _apiKey);
				var request = await HttpClient.GetAsync(url);
				var jsonString = await request.Content.ReadAsStringAsync();
				List<TDb> items = new List<TDb>();
				if (!string.IsNullOrWhiteSpace(jsonString))
				{
					items = JsonConvert.DeserializeObject<List<TDb>>(jsonString);
					items = items.Select(item =>
					{
						item.ProductId = Common.Utils.Util.GetProductId(productsWithIds, item, ArticlesToIgnore);
						item.UploadDate = DateTime.Now;
						return item;
					}).ToList();
				}
				else
				{
					_logger.LogWarning($"[Wb][API] no data for {typeof(TDb).Name} for {currentDate}");
				}
				var existing = _context.Set<TDb>().Where(sale => sale.Date.Date == currentDate.Date);
				_context.RemoveRange(existing);
				await _context.AddRangeAsync(items.Where(prod => prod.ProductId.HasValue));
				await _context.SaveChangesAsync();
				return items;
			}
			catch (Exception ex)
			{
				var message = ex.ToString();
				_logger.LogError(message);
				NotifyCantParse(message);
				throw;
			}
		}

		private void NotifyCantParse(string ex)
		{
			Emails.ForEach(email => _emailSender.SendEmail(email, $"{DataTypeStr} Ошибка при получении данных по АПИ ВБ", ex));
		}
	}
}