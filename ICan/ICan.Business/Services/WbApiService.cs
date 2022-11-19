using AutoMapper;
using ICan.Data.Context;
using ICan.Business.Managers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ICan.Common.Models.WB;
using ICan.Common.Domain;
using ICan.Common.Models.Opt;
using ICan.Common.Models.Exceptions;
using ICan.Common.Utils;

namespace ICan.Business.Services
{
	public class WbApiService
	{
	
		private readonly ILogger<WbApiService> _logger;
		private readonly IEmailSender _sender;
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;

		private readonly string SalesApiUrl;
		private readonly string OrdersApiUrl;
		private readonly string WarehouseApiUrl;

		private readonly ProductManager _productManager;
		private static string _datesFormat = "yyyy-MM-ddTHH:mm:ss";

		private readonly string _apiKey;

		private static readonly HttpClient HttpClient = new HttpClient();

		public IEnumerable<string> ArticlesToIgnore { get; }

		public WbApiService(ApplicationDbContext context, IConfiguration configuration,
			IMapper mapper,
			IEmailSender sender,
			ILogger<WbApiService> logger, ProductManager productManager)
		{
			_apiKey = configuration["Settings:WB:ApiKey"];

			_logger = logger;
			_productManager = productManager;
			_context = context;
			_mapper = mapper;
			_sender = sender;
			SalesApiUrl = configuration["Settings:WB:SalesApiUrl"];
			OrdersApiUrl = configuration["Settings:WB:OrdersApiUrl"];
			WarehouseApiUrl = configuration["Settings:WB:WarehouseApiUrl"];
			var raw = configuration["Settings:WB:ArticlesToIgnore"];

			ArticlesToIgnore = string.IsNullOrWhiteSpace(raw) ? Enumerable.Empty<string>() : raw.Split(",");
		}


		public async Task<WarehouseReport> GetWarehouseReport(WarehouseReport report)
		{
			report.WarehouseItems = await GetWarehouseStateAsync();
			report.Orders = await GetOrders(report);
			return report;
		}

		public async Task<IEnumerable<IWbItemModel>> GetOrders(WarehouseReport report)
		{
			return await GetItems<OptWbOrder, WbOrderModel>(OrdersApiUrl, report);
		}

		public IEnumerable<IWbItemModel> GetWbItems<T>(WarehouseReport report)
			 where T : class, IWBItemWithProductId
		{
			var data = _context.Set<T>()
					.Where(item => item.Date <= report.MaxDate && item.Date >= report.MinDate).ToList();
			var list = _mapper.Map<IEnumerable<IWbItemModel>>(data);
			return list;
		}

		public async Task<IEnumerable<IWbItemModel>> GetSales(WarehouseReport report)
		{
			return await GetItems<OptWbSale, WbSaleModel>(SalesApiUrl, report);
		}

		private async Task<IEnumerable<IWbItemModel>> GetWarehouseStateAsync()
		{
			var now = DateTime.Now.Date;
			var nowdateFrom = now.ToString(_datesFormat);

			var whUrl = string.Format(WarehouseApiUrl, nowdateFrom, _apiKey);
			var request = await HttpClient.GetAsync(whUrl);

			if (request.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
			{
				throw new UserException("Слишком много запросов в минуту. Необхожимо подождать");
			}

			var jsonString = await request.Content.ReadAsStringAsync();
			var whItems = JsonConvert.DeserializeObject<IEnumerable<OptWbWarehouse>>(jsonString);
			var productsWithIds = await _productManager.GetProductsWithIsbn();
			whItems = whItems.Select(item =>
			{
				item.ProductId = Util.GetProductId(productsWithIds, item, ArticlesToIgnore);
				item.UploadDate = DateTime.Now;
				return item;
			});
			var list = _mapper.Map<IEnumerable<IWbItemModel>>(whItems
				.Where(prod => prod.ProductId.HasValue)
				.ToList());
			return list;
		}

	
		private async Task<IEnumerable<IWbItemModel>> GetItems<I, T>(string url, WarehouseReport report)
			 where I : class, IWBItemWithProductId
			where T : class
		{
			var items = new List<IWbItemModel>();
			var startingDate = report.MinDate;
			//	DateTime.Now.AddDays(-1).Date; //don't need to include this day into report 
			DateTime date = startingDate;

			for (int i = 1; date.Date <= report.MaxDate.Date; i++)
			{
				var existing = _context.Set<I>().Where(item => item.Date.Date == date.Date).AsEnumerable();
	 
				items.AddRange(_mapper.Map<IEnumerable<IWbItemModel>>(existing));
				date = startingDate.AddDays(i);
			}
			return items;
		}

	}
}
