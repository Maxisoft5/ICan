using ICan.Extensions;
using ICan.Business.Managers;
using ICan.Common.Models;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.WB;
using ICan.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using ICan.Common.Utils;
using ICan.Common;
using ICan.Common.Domain;

namespace ICan.Controllers
{
	[Authorize(Roles = "Admin,Operator,Storekeeper,Assembler")]
	[ServiceFilter(typeof(SetRoleAttribute))]
	public partial class WbController : BaseController
	{
		private readonly ReportManager _reportManager;
		private readonly WbManager _wbManager;
		private readonly ProductManager _productManager;
		private readonly WbApiService _wbApiService;
		private readonly DateTime ConsolidateReportPeriodFrom;
		public readonly string SalesApiUrl;
		public readonly string OrdersApiUrl;

		public WbController(
				ReportManager reportManager,
				ILogger<BaseController> logger,
				IConfiguration configuration,
				WbApiService wbApiService,
				WbManager wbManager,
				ProductManager productManager
			)
			: base(logger)
		{
			ConsolidateReportPeriodFrom = DateTime.Parse(configuration["Settings:ConsolidateReportPeriodFrom"]);
			_reportManager = reportManager;
			_wbApiService = wbApiService;
			_wbManager = wbManager;
			_productManager = productManager;
			SalesApiUrl = configuration["Settings:WB:SalesApiUrl"];
			OrdersApiUrl = configuration["Settings:WB:OrdersApiUrl"];
		}

		public IActionResult Reports() => View();

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UploadReport(IFormCollection collection)
		{
			if (collection.Files == null || collection.Files.Count == 0)
			{
				_logger.LogError(Const.ErrorMessages.NoFile);
				TempData["ErrorMessage"] = Const.ErrorMessages.NoFile;
			}
			else
			{
				try
				{
					await _reportManager.UploadReport(collection);

					TempData["StatusMessage"] = "Отчёт успешно загружен";
				}
				catch (UserException ex)
				{
					_logger.LogError(ex, "ой");
					TempData["ErrorMessage"] = ex.Message;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "ой");
					TempData["ErrorMessage"] = "При загрузке отчёта возникли проблемы";
				}
			}
			return RedirectToAction("Reports");
		}

		public async Task<IActionResult> DownloadConsolidatedReport()
		{
			var report = await _reportManager.GetConsolidatedWBReport(ConsolidateReportPeriodFrom);
			return File(report, MediaTypeNames.Application.Octet,
				$"Сводный отчет от {ConsolidateReportPeriodFrom.ToShortDateString()} по {DateTime.Now.ToShortDateString()}.xlsx");
		}

		public IActionResult ApiReports()
		{
			var model = new WbFilterModel { StartDate = DateTime.Now.AddMonths(-1), EndDate = DateTime.Now };
			var list = new List<WbReportProductModel>();
			var products = _productManager.GetNotebooks()
				.Where(product => !product.CountryId.HasValue)
				.Select(product => new WbReportProductModel
				{
					DisplayName = product.DisplayName,
					ProductId = product.ProductId,
				});
			list.AddRange(products);
			model.Products = list.ToArray();
			model.WarehouseNames =
				WbManager.WhNames.Select(whName =>
			{
				var value = string.Join(",", whName);
				return new WbReportWarehouseModel
				{
					DisplayName = value,
				};
			}).ToArray();
			model.ReportTypes = new WbReportTypeModel[]
			{
				new WbReportTypeModel
				{
					DisplayName= WbReportType.Orders.GetDisplayName(),
					ReportType = WbReportType.Orders,
				},
				new WbReportTypeModel
				{
					DisplayName= WbReportType.Sales.GetDisplayName(),
					ReportType = WbReportType.Sales,
				},
			};
			WbManager.WhNames.Select(whName =>
		{
			var value = string.Join(",", whName);
			return new WbReportWarehouseModel
			{
				DisplayName = value,
			};
		}).ToArray();

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult GetGraphicsData(WbFilterModel filter)
		{
			try
			{
				var categories = new List<string>();
				var report = new WarehouseReport { MinDate = filter.StartDate, MaxDate = filter.EndDate };
				var items = new List<IWbItemModel>();

				if (filter.SelectedReportTypes.Contains((int)WbReportType.Orders))
				{
					var orders = _wbApiService.GetWbItems<OptWbOrder>(report)
						.Select(order =>
						{
							order.ReportType = WbReportType.Orders;
							return order;
						});
					items.AddRange(orders);
				}
				if (filter.SelectedReportTypes.Contains((int)WbReportType.Sales))
				{
					var sales = _wbApiService.GetWbItems<OptWbSale>(report)
							.Select(sale =>
							{
								sale.ReportType = WbReportType.Sales;
								return sale;
							});
					items.AddRange(sales);
				}

				if (filter.SelectedProducts.Any())
				{
					items = items.Where(item => filter.SelectedProducts.Contains(item.ProductId)).ToList();
				}

				var whNames = filter.SelectedWarehouseNames.SelectMany(wh => wh.Split(","));
				items = items.Where(item => whNames.Contains(item.WarehouseName)).ToList();
				var series = PrepareData(filter, categories, items);
				return Ok(new { categories, series });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[WB][Graph]");
				return BadRequest();
			}
		}

		//[HttpPost]
		//[ValidateAntiForgeryToken]
		public async Task<IActionResult> GatherWarehouseData()
		{
			try
			{
				var maxDate = DateTime.Now.AddDays(-1).Date;
				var report = new WarehouseReport
				{
					MinDate = maxDate.AddDays(-Const.WbDaysInScope),
					MaxDate = maxDate,
				};
				var apiData = await _wbApiService.GetWarehouseReport(report);
				var bytes = await _wbManager.GetWbWarehouseReportFile(apiData);
				return File(bytes, MediaTypeNames.Application.Octet, $"Поставка в ВБ {DateTime.Now.ToShortDateString()}.xlsx");
			}
			catch (UserException ex)
			{
				_logger.LogWarning($"[Wb][API] {ex.Message}");
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[WbApi][Warehouse]");
				TempData["ErrorMessage"] = "При формировании отчёта возникли проблемы";
			}

			return RedirectToAction(nameof(ApiReports));
		}

		public IActionResult CompareReport() => View();

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CompareReport(IFormFile formFile)
		{
			try
			{
				var fileData = await _wbManager.ParseWbSalesReport(formFile);
				var dates = fileData.Select(item => item.SaleDate);
				var report = new WarehouseReport
				{
					MinDate = dates.Min(),
					MaxDate = dates.Max(),
				};

				var orders = await _wbApiService.GetOrders(report);
				var sales = await _wbApiService.GetSales(report);

				byte[] compareResult = _wbManager.CompareApiAndFile(fileData, orders, sales);
				return File(compareResult, MediaTypeNames.Application.Octet, $"Сравнение ЛК и API c {report.MinDate.ToShortDateString()} по {report.MaxDate.ToShortDateString()}.xlsx");
			}
			catch (UserException ex)
			{
				_logger.LogWarning($"[Wb][API] {ex.Message}");
				TempData["ErrorMessage"] = ex.Message;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[WbApi][Warehouse]");
				TempData["ErrorMessage"] = "При формировании отчёта возникли проблемы";
			}

			return RedirectToAction(nameof(CompareReport));
		}

		private bool CheckFiles(IFormCollection collection)
		{
			var error =
			collection.Files == null
					|| collection.Files.Count != 2
					|| (collection.Files.Count(File => File.FileName.EndsWith("xlsx")) != 1)
					|| (collection.Files.Count(File => File.FileName.EndsWith("pdf")) != 1);
			return !error;
		}

		private IEnumerable<ChartSeries> PrepareData(WbFilterModel filter, List<string> categories, IEnumerable<IWbItemModel> items)
		{
			var ordersSeries = new Dictionary<int, List<int>>();
			var salesSeries = new Dictionary<int, List<int>>();

			foreach (var productId in filter.SelectedProducts)
			{
				ordersSeries.Add(productId, new List<int>());
				salesSeries.Add(productId, new List<int>());
			}

			var date = filter.StartDate;
			while (date <= filter.EndDate)
			{
				categories.Add(date.ToString("dd.MM"));

				//foreach (var productId in filter.SelectedProducts)
				//{
				//	ordersSeries[productId].Add(orders.Where(item => item.Date.Date == date.Date
				//		&& item.ProductId == productId
				//	).Select(item => item.Quantity).Sum());
				//	salesSeries[productId].Add(sales.Where(item => item.Date.Date == date.Date
				//		&& item.ProductId == productId
				//	).Select(item => item.Quantity).Sum());
				//}

				date = date.AddDays(1);
			}
			var graphSeries = new List<ChartSeries> { };
			foreach (var productId in filter.SelectedProducts)
			{
				var product = filter.Products.First(product => product.ProductId == productId);
				foreach (var whName in filter.SelectedWarehouseNames)
				{
					var selectedWh = whName.Split(",");
					foreach (var reportType in filter.SelectedReportTypes)
					{
						var enumVal = Enum.Parse<WbReportType>(reportType.ToString());
						date = filter.StartDate;
						List<int> currentSeries = new List<int>();
						var selectedItems = items.Where(item => item.ProductId == productId &&
							whName.Contains(item.WarehouseName) && item.ReportType == enumVal).ToList();
						while (date <= filter.EndDate)
						{
							var dayQuantity = selectedItems.Where(item => item.Date.Date == date.Date)
								.Sum(item => item.Quantity);
							currentSeries.Add(dayQuantity);
							date = date.AddDays(1);
						}

						graphSeries.Add(new ChartSeries { Name = $"{enumVal.GetDisplayName()} {whName} {product.DisplayName}", Data = currentSeries.ToArray() });
					}
				}
			}

			return graphSeries;
		}
	}
}