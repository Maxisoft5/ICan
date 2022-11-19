using ICan.Common.Domain;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Common.Models.Opt.Report;
using ICan.Common.Models.WB;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ICan.Common.Utils
{
	public static class ExcelUtil
	{
		public static Dictionary<WbCity, (string CityName, int AmountCell, string AmountColumn)> CityDetails =
		 new Dictionary<WbCity, (string CityName, int AmountCell, string AmountColumn)> {
				 { WbCity.Podolsk, ("Подольск", 6,"G")},
				 { WbCity.Domodedovo, ("Домодедово", 15,"P")},
				 { WbCity.Pushkino, ("Пушкино", 15,"P")},
				 { WbCity.Novosibirsk, ("Новосибирск", 7,"H")},
				 { WbCity.Khabarovsk, ("Хабаровск",8,"I")},
				 { WbCity.Krasnodar, ("Краснодар", 9,"J")},
				 { WbCity.Ekaterinburg, ("Екатеринбург", 10,"K")},
				 { WbCity.SaintPetersburg, ("Санкт-Петербург", 11,"L")},
				 { WbCity.Kazan, ("Казань", 14,"O")}
		 };

		public static void AddWorkSheet(string title,
				ExcelWorkbook workbook,
				IEnumerable<ReportItemModel> reportItems,
				ReportHelper helper,
				IEnumerable<WarehouseItemModel> warehouseItems = null,
				bool showRemainigs = true,
				bool showPaid = false,
				IEnumerable<OptOrderpayment> orderPayments = null)
		{
			var sheet = workbook.Worksheets.Add(title);

			PrintHeader(sheet, helper, showRemainigs);
			PrintContents(sheet, helper, reportItems, Enumerable.Empty<ReportItemModel>(), warehouseItems, showRemainigs);
			PrintCommonFooter(sheet, helper, reportItems, showPaid, orderPayments);

			sheet.Cells.AutoFitColumns(5.0, 40.0);
		}

		public static void SwapColumns(int columnFrom, int columnTo, ExcelWorksheet worksheet)
		{
			var lastRaw = worksheet.Dimension.Rows + 1;
			for (var raw = 1; raw < lastRaw; raw++)
			{
				var temp = worksheet.Cells[raw, columnFrom].Value;
				worksheet.Cells[raw, columnFrom].Value = worksheet.Cells[raw, columnTo].Value;
				worksheet.Cells[raw, columnTo].Value = temp;
			}
		}

		public static void PrintSumByColumns(List<int> columns, ExcelWorksheet worksheet)
		{
			var column = CityDetails.Count + 3;
			var colFromHex = ColorTranslator.FromHtml("#666699");
			worksheet.Cells[1, column].Value = "Сумма по МСК";
			worksheet.Cells[1, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
			worksheet.Cells[1, column].Style.Fill.BackgroundColor.SetColor(colFromHex);
			worksheet.Cells[1, column].Style.WrapText = true;
			worksheet.Cells[1, column].Style.Font.Bold = true;
			worksheet.Cells[1, column].Style.Font.Color.SetColor(Color.White);
			worksheet.Cells[1, column].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
			worksheet.Cells[1, column].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
			worksheet.Column(column).Width = 14;

			var lastRaw = worksheet.Dimension.Rows + 1;
			for (var raw = 2; raw < lastRaw; raw++)
			{
				var sum = 0;
				string value = "";
				foreach (var col in columns)
				{
					value = worksheet.Cells[raw, col].Value?.ToString();
					if (value != null)
						sum += int.Parse(value);
				}
				if (value != null)
					worksheet.Cells[raw, column].Value = sum;
			}
		}

		public static void PrintHeader(ExcelWorksheet objWorksheet, ReportHelper helper, bool showRemainigs = true)
		{
			var currentDate = helper.StartDate;
			var i = 0;
			do
			{
				objWorksheet.Cells[helper.StartRow, helper.StartMonthColumn + i].Value
					= helper.Months.First(t => t.Value == currentDate.Month.ToString()).Text;
				i++;
				currentDate = currentDate.AddMonths(1);
			} while (currentDate != helper.EndDate);
			if (showRemainigs)
				objWorksheet.Cells[helper.StartRow, helper.StartMonthColumn + i].Value = "Текущий остаток";
		}

		public static void PrintLeonardoHeader(IEnumerable<ReportProductModel> products, int startLeonardoColumn, ExcelWorksheet objWorksheet, LeonardoAggReport[] list)
		{
			var startRow = 3;

			objWorksheet.PrinterSettings.Orientation = eOrientation.Landscape;
			objWorksheet.PrinterSettings.FitToPage = true;
			objWorksheet.PrinterSettings.FitToHeight = 1;
			objWorksheet.PrinterSettings.FooterMargin = .05M;
			objWorksheet.PrinterSettings.TopMargin = .05M;
			objWorksheet.PrinterSettings.LeftMargin = .05M;
			objWorksheet.PrinterSettings.RightMargin = .05M;

			PrintLeoardoProducts(products, startRow, startLeonardoColumn, objWorksheet);

			int i = startRow;
			int j = startLeonardoColumn + 3;
			int maxRow = objWorksheet.Dimension.Rows + 2;
			LeonardoAggReport report;

			for (var k = 0; k < list.Count(); k++)
			{
				report = list[k];
				i = startRow;
				if (k > 0 && k % 7 == 0)
				{
					j = startLeonardoColumn + 3;
					startRow = (objWorksheet.Dimension.Rows + 3);
					i = startRow;
					objWorksheet.Row(i - 3).PageBreak = true;
					PrintLeoardoProducts(products, i, startLeonardoColumn, objWorksheet);
					maxRow = products.Count();
				}

				objWorksheet.Cells[i - 2, j].Value = report.LocalShopName;
				objWorksheet.Cells[i - 2, j].Style.WrapText = true;
				objWorksheet.Cells[i - 1, j].Value = report.TabName;
				objWorksheet.Cells[i - 1, j].Style.WrapText = true;
				objWorksheet.Cells[i, j].Value = "Заказ";
				i++;
				maxRow = i + products.Count();
				for (; i <= maxRow - 1; i++)
				{
					var isbn = objWorksheet.Cells[i, startLeonardoColumn + 2].Value.ToString();
					var amount = report.ReportItems.FirstOrDefault(t => t.ISBN == isbn)?.Amount.ToString() ?? "";
					objWorksheet.Cells[i, j].Value = amount;
				}

				objWorksheet.Cells[i, j].Value = report.ReportItems.Sum(t => t.Amount);
				j++;
			}
			objWorksheet.Column(1).Width = 45;
			objWorksheet.Column(2).Width = 15;
		}

		public static byte[] GetCompareReport(IOrderedEnumerable<CompareWbResultItem> results)
		{
			using (ExcelPackage objExcelPackage = new ExcelPackage())
			{
				ExcelWorksheet objWorksheet = objExcelPackage.Workbook.Worksheets.Add("Общий");
				var i = 1;
				var j = 1;
				objWorksheet.Cells[i, j++].Value = "Дата";
				objWorksheet.Cells[i, j++].Value = "Заказы API";
				objWorksheet.Cells[i, j++].Value = "Заказы ЛК";
				objWorksheet.Cells[i, j++].Value = "Заказы Разница";

				objWorksheet.Cells[i, j++].Value = "Продажи API";
				objWorksheet.Cells[i, j++].Value = "Продажи ЛК";
				objWorksheet.Cells[i, j].Value = "Продажи Разница";

				objWorksheet.Cells[i, 1, i, j].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
				objWorksheet.Cells[i, 1, i, j].AutoFitColumns();
				i++;
				foreach (var result in results)
				{
					j = 1;
					objWorksheet.Cells[i, j++].Value = result.Date.ToShortDateString();
					objWorksheet.Cells[i, j++].Value = result.OrdersFromApi;
					objWorksheet.Cells[i, j++].Value = result.OrdersFromFile;
					objWorksheet.Cells[i, j].Value = result.OrdersDiff;
					if (result.OrdersDiff.HasValue && result.OrdersDiff != 0)
					{
						objWorksheet.Cells[i, j].Style.Font.Color.SetColor(Color.Red);
					}
					j++;

					objWorksheet.Cells[i, j++].Value = result.SalesFromApi;
					objWorksheet.Cells[i, j++].Value = result.SalesFromFile;
					objWorksheet.Cells[i, j].Value = result.SalesDiff;
					if (result.SalesDiff.HasValue && result.SalesDiff != 0)
					{
						objWorksheet.Cells[i, j].Style.Font.Color.SetColor(Color.Red);
					}
					i++;
				}

				var bytes = objExcelPackage.GetAsByteArray();
				return bytes;
			}
		}

		private static void PrintLeoardoProducts(IEnumerable<ReportProductModel> products, int startRow, int startColumn, ExcelWorksheet objWorksheet)
		{
			var i = startRow;
			var j = startColumn;
			objWorksheet.Cells[startRow, startColumn].Value = "Товар";
			objWorksheet.Cells[startRow, startColumn + 1].Value = "Наименование";
			objWorksheet.Cells[startRow, startColumn + 2].Value = "Штрих_код наименования";

			i++;

			foreach (var product in products)
			{
				objWorksheet.Cells[i, startColumn].Value = product.Name;

				objWorksheet.Cells[i, startColumn + 1].Value = product.SeriesName;

				objWorksheet.Cells[i, startColumn + 2].Value = product.ISBN;
				i++;
			}
			objWorksheet.Column(1).Style.Font.Size = 9;
			objWorksheet.Column(2).Style.Font.Size = 9;
		}

		public static void PrintWbProducts(IDictionary<OptProductseries, List<ProductModel>> productGroups, ExcelWorksheet objWorksheet, IEnumerable<WbAggregateReport> reports)
		{
			int startRow = 1, startColumn = 0;
			var i = startRow;
			var productColumn = 2;

			var reportDictionary = reports.ToDictionary(report => report.City, report => report);

			PrintWbHeader(objWorksheet);

			foreach (var group in productGroups)
			{
				objWorksheet.Cells[i, productColumn].Value = group.Key.Name;
				i++;
				foreach (var product in group.Value)
				{
					objWorksheet.Cells[i, productColumn - 1].Value = product.ISBN;
					objWorksheet.Cells[i, productColumn].Value = product.DisplayName;
					int j = 3;
					foreach (var city in CityDetails)
					{

						var report = reportDictionary[city.Key];
						var reportProduct = report.ReportItems.FirstOrDefault(it => it.NoteBookId == product.ProductId);

						if (reportProduct != null)
							objWorksheet.Cells[i, startColumn + j].Value = reportProduct.Amount;
						j++;
					}
					i++;
				}
			}
			objWorksheet.Column(1).Width = 15;
			objWorksheet.Column(2).Width = 47;
		}

		public static void PrintWbProducts(IDictionary<OptProductseries, List<ProductModel>> productGroups, ExcelWorksheet objWorksheet, WarehouseReport apiData)
		{
			int startRow = 1, startColumn = 0;
			var i = startRow;
			var productColumn = 2;

			PrintWbHeader(objWorksheet);

			foreach (var group in productGroups)
			{
				objWorksheet.Cells[i, productColumn].Value = group.Key.Name;
				i++;
				foreach (var product in group.Value)
				{
					objWorksheet.Cells[i, productColumn - 1].Value = product.ISBN;
					objWorksheet.Cells[i, productColumn].Value = product.DisplayName;
					int j = 3;
					var apiProductInfo = apiData.WarehouseItems
						.Where(item => item.ProductId == product.ProductId);

					foreach (var city in CityDetails)
					{
						var cityApiData = apiProductInfo.Where(item =>
							!string.IsNullOrWhiteSpace(item.WarehouseName) &&
							item.WarehouseName.Equals(city.Value.CityName));

						var productAmountInCity = cityApiData.Sum(item => item.Quantity);
						objWorksheet.Cells[i, startColumn + j].Value = productAmountInCity;

						j++;
					}
					i++;
				}
			}
			objWorksheet.Column(1).Width = 15;
			objWorksheet.Column(2).Width = 47;
		}

		public static void PrintWbWarehouseAndSales(string city, Dictionary<OptProductseries, List<ProductModel>> productGroups, ExcelWorkbook workBook, WarehouseReport apiData)
		{
			ExcelWorksheet objWorksheet = workBook.Worksheets.Add(city);

			PrintWbWarehouseOrdersHeader(objWorksheet, apiData);
			var whCityData = apiData.WarehouseItems.Where(item =>
				!string.IsNullOrWhiteSpace(item.WarehouseName)
				&& item.WarehouseName.Equals(city, StringComparison.InvariantCultureIgnoreCase));
			var ordersCityData = apiData.Orders.Where(item => !string.IsNullOrWhiteSpace(item.WarehouseName)
				&& item.WarehouseName.Equals(city, StringComparison.InvariantCultureIgnoreCase));

			DateTime date = apiData.MaxDate;
			var i = 2;
			var productColumn = 2;

			foreach (var group in productGroups)
			{
				objWorksheet.Cells[i, productColumn].Value = group.Key.Name;
				i++;
				foreach (var product in group.Value)
				{
					objWorksheet.Cells[i, productColumn - 1].Value = product.ISBN;
					objWorksheet.Cells[i, productColumn].Value = product.DisplayName;
					int j = 3;

					// остатки
					var productInCityRemainigs = whCityData.Where(item =>
						item.ProductId == product.ProductId).Sum(item => item.Quantity);
					objWorksheet.Cells[i, j++].Value = productInCityRemainigs;

					// заказы  
					date = apiData.MaxDate;
					for (int dateInd = 0; date > apiData.MinDate; dateInd++)
					{
						date = apiData.MaxDate.AddDays(-dateInd);
						var productItems = ordersCityData.Where(prod => prod.ProductId == product.ProductId);
						var items = productItems.Where(prod => prod.Date.Date == date.Date);

						objWorksheet.Cells[i, j++].Value = items.Sum(prod => prod.Quantity);
					}
					i++;
				}
			}
			objWorksheet.Column(1).Width = 15;
			objWorksheet.Column(2).Width = 47;
		}

		private static void PrintWbWarehouseOrdersHeader(ExcelWorksheet objWorksheet, WarehouseReport apiData)
		{
			var j = 3;
			objWorksheet.Cells[1, j++].Value = "Остаток";
			var now = DateTime.Now.Date.AddDays(-1);
			var date = apiData.MaxDate;
			for (int i = 0; date > apiData.MinDate; i++)
			{
				date = apiData.MaxDate.AddDays(-i);
				objWorksheet.Cells[1, j].Value = date.ToShortDateString();
				objWorksheet.Cells[1, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
				objWorksheet.Cells[1, j].Style.Numberformat.Format = "mm-dd-yy";

				objWorksheet.Column(j).Width = 12;
				j = j + 1;
			}
		}

		public static byte[] PrintContents(ReportHelper helper, Dictionary<int, string> shops,
			IEnumerable<ReportItemModel> reportItems)
		{
			var includedMonth = helper.FromYear == DateTime.Now.Year ? DateTime.Now.Month : 12;
			var groupedReports = reportItems.GroupBy(x => x.ReportMonth).OrderBy(x => x.Key).ToList();
			using (ExcelPackage objExcelPackage = new ExcelPackage())
			{
				for (int month = 1; month <= includedMonth; month++)
				{
					ExcelWorksheet objWorksheet = objExcelPackage.Workbook.Worksheets.Add($"Отчёт за {DateTimeFormatInfo.CurrentInfo.GetMonthName(month)}");
					var currentRow = 1;
					int j = 0;

					for (int k = 0; k < shops.Count(); k++, j++)
					{
						objWorksheet.Cells[currentRow, j + 2].Value = shops.ElementAt(k).Value;
					}

					objWorksheet.Cells[currentRow, j + 2].Value = "СП";
					j++;
					objWorksheet.Cells[currentRow, j + 2].Value = "Магазины";
					j++;

					var i = 0;
					foreach (var group in helper.Products)
					{
						if (group.Key.ProductKindId != 1)
							continue;
						currentRow++;
						objWorksheet.Cells[currentRow, 1].Value = group.Key.Name;
						objWorksheet.Cells[currentRow, 1].Style.Border.Right.Style = ExcelBorderStyle.Medium;
						objWorksheet.Cells[currentRow, 1].Style.Font.Bold = true;
						foreach (var product in group.Value)
						{
							currentRow++;
							objWorksheet.Cells[currentRow, 1].Value = product.DisplayName;
							objWorksheet.Cells[currentRow, 1].Style.Border.Right.Style = ExcelBorderStyle.Medium;
							i = 0;

							foreach (var shop in shops)
							{
								objWorksheet.Cells[currentRow, 2 + i].Value = GetProductTotalAmount(reportItems, product.ProductId, month, helper.FromYear, shop.Key);
								i++;
							}

							objWorksheet.Cells[currentRow, 2 + i].Value = GetProductTotalAmount(reportItems, product.ProductId, month, helper.FromYear, null, (int)ClientType.JointPurchase);
							i++;

							objWorksheet.Cells[currentRow, 2 + i].Value = GetProductTotalAmount(reportItems, product.ProductId, month, helper.FromYear, null, (int)ClientType.Shop);
							i++;

						}
					}
					objWorksheet.Column(1).Width = 50;
					for (var b = 2; b <= 11; b++)
					{
						objWorksheet.Column(b).Width = 15;
						objWorksheet.Cells[1, b].Style.Font.Bold = true;
						objWorksheet.Cells[1, b].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
						objWorksheet.Cells[1, b].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
					}
					objWorksheet.Cells[currentRow + 2, 1].Value = "Итого, руб";
					var columnNum = 2;
					foreach (var KVpair in shops)
					{
						objWorksheet.Cells[currentRow + 2, columnNum].Value = GetTotalSum(reportItems, KVpair.Key, month);
						columnNum++;
					}
					objWorksheet.Cells[currentRow + 2, columnNum].Value = GetTotalSumForInternal(reportItems, month, (int)ClientType.JointPurchase);
					columnNum++;
					objWorksheet.Cells[currentRow + 2, columnNum].Value = GetTotalSumForInternal(reportItems, month, (int)ClientType.Shop);
				}
				return objExcelPackage.GetAsByteArray();
			}
		}

		public static void PrintContents(ExcelWorksheet objWorksheet,
		ReportHelper helper,
		IEnumerable<ReportItemModel> commonReportItems,
		IEnumerable<ReportItemModel> updReportItems,
		IEnumerable<WarehouseItemModel> warehouseItems,
		bool showRemainigs = true)
		{
			var currentRow = helper.StartRow;
			var currentDate = helper.StartDate;
			var i = 0;

			foreach (var group in helper.Products)
			{
				if (group.Key.ProductKindId != 1)
					continue;
				currentRow++;
				objWorksheet.Cells[currentRow, 2].Value = group.Key.Name;
				foreach (var product in group.Value)
				{
					currentRow++;
					objWorksheet.Cells[currentRow, 1].Value = product.ISBN;
					objWorksheet.Cells[currentRow, 2].Value = product.DisplayName;
					i = 0;
					currentDate = helper.StartDate;
					do
					{
						objWorksheet.Cells[currentRow, helper.StartMonthColumn + i].Value =
							GetProductTotalAmount(commonReportItems, product.ProductId, currentDate.Month, currentDate.Year);
						i++;
						currentDate = currentDate.AddMonths(1);
					} while (currentDate != helper.EndDate);

					if (showRemainigs)
					{
						objWorksheet.Cells[currentRow, helper.StartMonthColumn + i].Value = GetRemainigs(product, warehouseItems, commonReportItems, updReportItems);
					}
				}
			}
		}

		public static void PrintFooter(ExcelWorksheet objWorksheet, ReportHelper helper, IEnumerable<OptReport> reports, bool printPaidSum)
		{
			var currentRow = objWorksheet.Dimension.Rows + 3;
			var totalWords = printPaidSum ? "Продано, руб" : "Итого, руб.";

			objWorksheet.Cells[currentRow, 2].Value = totalWords;

			if (printPaidSum)
			{
				objWorksheet.Cells[currentRow + 1, 2].Value = "Выплачено, руб";
			}

			var currentDate = helper.StartDate;
			var i = 0;

			do
			{
				var report = reports.FirstOrDefault(t => t.ReportMonth == currentDate.Month
						&& t.ReportYear == currentDate.Year);
				if (report != null)
				{
					objWorksheet.Cells[currentRow, helper.StartMonthColumn + i].Value = report.TotalSum;
					objWorksheet.Cells[currentRow, helper.StartMonthColumn + i].Style.Numberformat.Format = "#,##0.00";

					if (printPaidSum)
						objWorksheet.Cells[currentRow + 1, helper.StartMonthColumn + i].Value = report.PaidSum;
					objWorksheet.Cells[currentRow + 1, helper.StartMonthColumn + i].Style.Numberformat.Format = "#,##0.00";
				}
				i++;
				currentDate = currentDate.AddMonths(1);
			} while (currentDate != helper.EndDate);
		}

		public static void PrintCommonFooter(ExcelWorksheet objWorksheet, ReportHelper helper, IEnumerable<ReportItemModel> reportItems, bool showPaid = false, IEnumerable<OptOrderpayment> orderPaments = null)
		{
			var currentRow = objWorksheet.Dimension.Rows + 3;
			objWorksheet.Cells[currentRow, 2].Value = "Итого, шт.";

			if (showPaid)
			{
				objWorksheet.Cells[currentRow + 1, 2].Value = "Оплачено";
			}
			var currentDate = helper.StartDate;
			var i = 0;

			do
			{
				var currentItemSet = reportItems.Where(t => t.ReportMonth == currentDate.Month
						&& t.ReportYear == currentDate.Year);
				if (currentItemSet.Count() > 0)
					objWorksheet.Cells[currentRow, helper.StartMonthColumn + i].Value = currentItemSet.Sum(t => t.Amount);
				else
					objWorksheet.Cells[currentRow, helper.StartMonthColumn + i].Value = 0;
				if (showPaid)
				{
					var paymentSum =
						orderPaments.Where(t => t.OrderPaymentDate.Month == currentDate.Month && t.OrderPaymentDate.Year == currentDate.Year).Sum(t => t.Amount);

					objWorksheet.Cells[currentRow + 1, helper.StartMonthColumn + i].Value = paymentSum;
					objWorksheet.Cells[currentRow + 1, helper.StartMonthColumn + i].Style.Numberformat.Format = "#,##0.00";
				}
				i++;
				currentDate = currentDate.AddMonths(1);
			} while (currentDate != helper.EndDate);
		}

		public static List<PackageInfo> ParsePackageInfoAsync(Stream fileStream, List<PackageInfo> list)
		{
			var idColumn = "E";
			var amountColumn = "F";

			using (var package = new ExcelPackage(fileStream))
			{
				var worksheet = package.Workbook.Worksheets[0];

				// вычитаем ряд заголовков
				if (worksheet.Dimension.Rows - 1 != list.Count)
				{
					throw new UserException(Const.ErrorMessages.WrongNumberOfPackages);
				}

				for (var i = 2; i <= worksheet.Dimension.Rows; i++)
				{
					var id = worksheet.Cells[$"{idColumn}{i}"].Value?.ToString();
					var amount = worksheet.Cells[$"{amountColumn}{i}"].Value?.ToString();
					var item = list.FirstOrDefault(packageInfo => packageInfo.Text.Equals(id, StringComparison.InvariantCultureIgnoreCase));
					if (item == null)
					{
						throw new UserException($"{Const.ErrorMessages.NoPackageInfo} {id}");
					}
					item.Count = int.Parse(amount);
				}
			}
			return list;
		}

		public static void AddWbSheet(Dictionary<OptProductseries, List<ProductModel>> products, IEnumerable<IWBItemWithProductId> reportItems, ExcelWorkbook workbook, string title, DateTime startDate, DateTime endDate)
		{
			var sheet = workbook.Worksheets.Add(title);

			var currentCol = 4;
			var currentRow = 1;
			var currentDate = startDate;
			sheet.Cells[currentRow, 1].Value = "Тетради";
			sheet.Column(1).Width = 54;
			while (currentDate.Date <= endDate.Date)
			{
				sheet.Column(currentCol).Width = 10;

				sheet.Cells[currentRow, currentCol++].Value = currentDate.ToShortDateString();
				currentDate = currentDate.AddDays(1);
			}
			foreach (var group in products)
			{
				if (group.Key.ProductKindId != 1)
					continue;
				currentRow++;
				sheet.Cells[currentRow, 1].Value = group.Key.Name;
			
				sheet.Cells[currentRow, 1].Style.Border.Right.Style = ExcelBorderStyle.Medium;
				sheet.Cells[currentRow, 1].Style.Font.Bold = true;
			
				foreach (var product in group.Value)
				{
					currentRow++;
					sheet.Cells[currentRow, 1].Value = product.DisplayName;
					sheet.Cells[currentRow, 2].Value = product.ISBN;
					sheet.Cells[currentRow, 3].Value = product.ArticleNumber;

					sheet.Cells[currentRow, 3].Style.Border.Right.Style = ExcelBorderStyle.Medium;
					currentDate = startDate;
					currentCol = 4;
					while (currentDate.Date <= endDate.Date)
					{
						var value = reportItems.Where(item => item.ProductId == product.ProductId && item.Date.Date == currentDate.Date).Select(notebook=> notebook.Quantity).Sum();
						sheet.Cells[currentRow, currentCol++].Value = value;
						currentDate = currentDate.AddDays(1);
					}
				}
			}
		}
		private static void PrintWbHeader(ExcelWorksheet objWorksheet)
		{
			int j = 3;
			var colFromHex = ColorTranslator.FromHtml("#666699");
			foreach (var city in CityDetails)
			{
				objWorksheet.Cells[1, j].Value = city.Value.CityName;
				objWorksheet.Column(j).Width = 14;
				j++;
			}
			j--;
			objWorksheet.Cells[1, 3, 1, j].Style.Fill.PatternType = ExcelFillStyle.Solid;
			objWorksheet.Cells[1, 3, 1, j].Style.Fill.BackgroundColor.SetColor(colFromHex);
			objWorksheet.Cells[1, 3, 1, j].Style.WrapText = true;
			objWorksheet.Cells[1, 3, 1, j].Style.Font.Bold = true;
			objWorksheet.Cells[1, 3, 1, j].Style.Font.Color.SetColor(Color.White);
			objWorksheet.Cells[1, 3, 1, j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
			objWorksheet.Cells[1, 3, 1, j].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
		}


		private static double GetTotalSum(IEnumerable<ReportItemModel> items, int shopId, int month)
		{
			var reportItem = items.Where(x => x.ReportMonth == month && x.ShopId == shopId).GroupBy(x => new { x.ReportId, x.ReportTotalSum });
			if (reportItem != null)
				return reportItem.Sum(x => x.Key.ReportTotalSum);
			return 0;
		}

		private static double GetTotalSumForInternal(IEnumerable<ReportItemModel> reportItems, int month, int clientType)
		{
			var totalSum = reportItems.Where(x => x.ReportMonth == month && x.ClientType == clientType)
							.GroupBy(g => g.OrderId)
							.Select(x => x.First())
							.Sum(x => x.ReportTotalSum);
			return totalSum;
		}

		private static int GetProductTotalAmount(IEnumerable<ReportItemModel> reportItems, int productId, int month, int year, int? shopId, int? clientType = null)
		{
			var amount = reportItems.Where(t => t.ProductId == productId
			&& t.ReportMonth == month
			&& t.ReportYear == year
			&& t.ShopId == shopId
			&& (clientType == null || t.ClientType == clientType))
				.Sum(t => t.Amount);
			return amount;
		}

		private static int GetProductTotalAmount(IEnumerable<ReportItemModel> reportItems, int productId, int month, int year)
		{
			var amount = reportItems.Where(t => t.ProductId == productId && t.ReportMonth == month && t.ReportYear == year).
				Sum(t => t.Amount);
			return amount;
		}

		private static int? GetRemainigs(ProductModel product, IEnumerable<WarehouseItemModel> warehouseItems, IEnumerable<ReportItemModel> commonReportItems, IEnumerable<ReportItemModel> updReportItems)
		{
			var initalState = warehouseItems.FirstOrDefault(item => item.ProductId == product.ProductId)?.Amount;
			if (initalState == null)
				return null;
			var plusState = updReportItems?.Where(item => item.ProductId == product.ProductId).Sum(t => t.Amount) ?? 0;
			var minusState = commonReportItems.Where(item => item.ProductId == product.ProductId).Sum(t => t.Amount);
			return initalState + plusState - minusState;
		}
	}
}
