using AutoMapper;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.WB;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ICan.Common.Utils;
using ICan.Data.Context;

namespace ICan.Business.Managers
{
	public class WbManager : BaseManager
	{
		private readonly ProductManager _productManager;

		public WbManager(IMapper mapper, ApplicationDbContext context,
			ProductManager productManager,
			ILogger<BaseManager> logger) : base(mapper, context, logger)
		{
			_productManager = productManager;
		}

		public static readonly IEnumerable<IEnumerable<string>> WhNames =
			new List<List<string>> { new List<string> { "Домодедово", "Подольск", "Пушкино" },
			new List<string> {"Екатеринбург"},
			new List<string> {"Казань"},
			new List<string> {"Краснодар"},
			new List<string> {"Новосибирск"},
			new List<string> {"Санкт-Петербург"},
			new List<string> { "Хабаровск" }
			};


		public async Task<byte[]> GetWbWarehouseReportFile(WarehouseReport apiData)
		{
			byte[] bytes = new byte[0];

			using (ExcelPackage objExcelPackage = new ExcelPackage())
			{
				ExcelWorksheet objWorksheet = objExcelPackage.Workbook.Worksheets.Add("Общий");

				var productGroups = await _productManager.GetAsync(false, dontShowDisabled: false, onlyNotebooks: true);

				ExcelUtil.PrintWbProducts(productGroups, objWorksheet, apiData);
				var cities = ExcelUtil.CityDetails.Select(city => city.Value.CityName).ToList();
				cities.ForEach(city => ExcelUtil.PrintWbWarehouseAndSales(city, productGroups, objExcelPackage.Workbook, apiData));
				ExcelUtil.PrintSumByColumns(new List<int> { 3, 4, 5 }, objWorksheet);
				ExcelUtil.SwapColumns(6, 12, objWorksheet);
				bytes = objExcelPackage.GetAsByteArray();
			}
			return bytes;
		}

		public async Task<IEnumerable<WeeklyRepotDataItem>> ParseWbSalesReport(IFormFile file)
		{
			List<WeeklyRepotDataItem> wbReportItems = new List<WeeklyRepotDataItem>();
			using (var memoryStream = new MemoryStream())
			{
				await file.CopyToAsync(memoryStream).ConfigureAwait(false);
				using (var package = new ExcelPackage(memoryStream))
				{
					var worksheet = package.Workbook.Worksheets.First();
					for (int i = 2; i <= worksheet.Dimension.Rows; i++)
					{
						var item = new WeeklyRepotDataItem
						{
							SaleDate = ParseDateCell(worksheet, i, WeeklyRepotDataItem.SaleDateCell),
							WeekNum = ParseIntCell(worksheet, i, WeeklyRepotDataItem.WeekNumCell),
							ArticleNumber = worksheet.Cells[$"{WeeklyRepotDataItem.ArticleNumberCell}{i}"].Value?.ToString(),
							SoldAmount = ParseIntCell(worksheet, i, WeeklyRepotDataItem.SoldAmountCell),
							AwardSum = ParseDecimalCell(worksheet, i, WeeklyRepotDataItem.AwardSumCell),
							OrderedAmount = ParseIntCell(worksheet, i, WeeklyRepotDataItem.OrderedAmountCell),
							OrderedSum = ParseDecimalCell(worksheet, i, WeeklyRepotDataItem.OrderedSumCell)
						};
						wbReportItems.Add(item);
					}
				}
			}
			return wbReportItems;
		}

		public byte[] CompareApiAndFile(IEnumerable<WeeklyRepotDataItem> fileData, IEnumerable<IWbItemModel> orders, IEnumerable<IWbItemModel> sales)
		{
			var results = GetCompareData(fileData, orders, sales);
			byte[] bytes = ExcelUtil.GetCompareReport(results.OrderBy(result => result.Date));
			return bytes;
		}

		private static List<CompareWbResultItem> GetCompareData(IEnumerable<WeeklyRepotDataItem> fileData, IEnumerable<IWbItemModel> orders, IEnumerable<IWbItemModel> sales)
		{
			var dates = fileData.Select(item => item.SaleDate);
			var minDate = dates.Min();
			var maxDate = dates.Max();
			var currentDate = minDate;
			var results = new List<CompareWbResultItem>();

			for (int i = 0; currentDate < maxDate; i++)
			{
				currentDate = minDate.AddDays(i);
				var thisDayFromFile = fileData.Where(item => item.SaleDate.Date.Equals(currentDate.Date));
				var thisDayApiOrders = orders.Where(order => order.Date.Date.Equals(currentDate.Date));
				var thisDayApiSales = sales.Where(sale => sale.Date.Date.Equals(currentDate.Date));
				results.Add(new CompareWbResultItem
				{
					Date = currentDate,
					OrdersFromApi = thisDayApiOrders.Count(),
					SalesFromApi = thisDayApiSales.Count(),
					OrdersFromFile = thisDayFromFile.Sum(item => item.OrderedAmount ),
					SalesFromFile = thisDayFromFile.Sum(item => item.SoldAmount)
				});
			}

			return results;
		}

		private decimal ParseDecimalCell(ExcelWorksheet worksheet, int i, string columnName)
		{
			if (decimal.TryParse(worksheet.Cells[$"{columnName}{i}"].Value?.ToString(), out var parsedValue))
			{
				return parsedValue;
			}
			else
			{
				throw new UserException($"Невозможно определить значение в столбце \"{worksheet.Cells[$"{columnName}"]}\" в строке {i}");
			}
		}

		private static DateTime ParseDateCell(ExcelWorksheet worksheet, int i, string columnName)
		{
			if (DateTime.TryParse(worksheet.Cells[$"{columnName}{i}"].Value?.ToString(), out var parsedValue))
			{
				return parsedValue;
			}
			else
			{
				throw new UserException($"Невозможно определить значение в столбце \"{worksheet.Cells[$"{columnName}"]}\" в строке {i}");
			}
		}

		private static int ParseIntCell(ExcelWorksheet worksheet, int i, string columnName)
		{
			if (int.TryParse(worksheet.Cells[$"{columnName}{i}"].Value?.ToString(), out var parsedValue))
			{
				return parsedValue;
			}
			else
			{
				throw new UserException($"Невозможно определить значение в столбце \"{worksheet.Cells[$"{columnName}"]}\" в строке {i}");
			}
		}
	}
}