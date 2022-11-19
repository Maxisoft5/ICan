using ICan.Common.Domain;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ICan.Common.Models.Opt.Report
{
	public class PrimerReport
	{
		protected readonly PrimerReportHelper _helper;
		protected int maxRow = 1;
		public PrimerReport(PrimerReportHelper helper)
		{
			_helper = helper;
		}

		public virtual byte[] GetReport()
		{
			byte[] result;
			using (var package = new ExcelPackage())
			{
				var includedMonth = _helper.Year == DateTime.Now.Year ? DateTime.Now.Month : 12;
				for (var month = 1; month <= includedMonth; month++)
				{
					var worksheet = package.Workbook.Worksheets.Add($"Отчёт за {DateTimeFormatInfo.CurrentInfo.GetMonthName(month)}");
					FillWorksheet(worksheet, month);
				}

				result = package.GetAsByteArray();
			}
			return result;
		}

		protected virtual void FillWorksheet(ExcelWorksheet worksheet, int month)
		{

			int row = maxRow = 1;
			PrintHeader(worksheet, row);

			var sums = PrintBody(row, month, worksheet);

			PrintFooter(sums, worksheet);
		}

		protected virtual List<double> PrintBody(int row, int month, ExcelWorksheet worksheet)
		{
			var sums = new List<double>();
			row++;
			sums.Add(FillOrderColumns("B", _helper.SPOrders.Where(x => x.DoneDate.Value.Month == month).ToList(), row, worksheet));
			sums.Add(FillOrderColumns("C", _helper.ShopOrders.Where(x => x.DoneDate.Value.Month == month).ToList(), row, worksheet));
			sums.Add(FillUpdColumns("D", _helper.OzonReportItems.Where(x => x.Report.ReportMonth == month && x.ProductId == _helper.ProductId).ToList(), row, worksheet));
			sums.Add(FillUpdColumns("E", _helper.WBReportItems.Where(x => x.Report.ReportMonth == month && x.ProductId == _helper.ProductId).ToList(), row, worksheet));
			sums.Add(FillUpdColumns("F", _helper.UPDReportItems.Where(x => x.Report.ReportMonth == month && x.ProductId == _helper.ProductId).ToList(), row, worksheet));
			return sums;
		}

		protected virtual void PrintFooter(List<double> sums, ExcelWorksheet worksheet)
		{
			worksheet.Cells["A" + maxRow].Value = $"Итого: {sums.Sum(x => x)}";
			worksheet.Cells["B" + maxRow].Value = sums[0];
			worksheet.Cells["C" + maxRow].Value = sums[1];
			worksheet.Cells["D" + maxRow].Value = sums[2];
			worksheet.Cells["E" + maxRow].Value = sums[3];
			worksheet.Cells["F" + maxRow].Value = sums[4];
			worksheet.Cells[maxRow, 1, maxRow, 6].Style.Font.Bold = true;
		}

		protected virtual double FillUpdColumns(string columnAddress, List<OptReportitem> data, int row, ExcelWorksheet worksheet)
		{
			double sum = 0;
			foreach (var reportItem in data)
			{
				worksheet.Cells[columnAddress + row].Value = $"Отчёт №{reportItem.Report.ReportNum} {reportItem.Report.ReportDate.Value.Date.ToShortDateString()}, " +
					$"Кол-во {reportItem.Amount}, " +
					$"Общая сумма: {reportItem.TotalSum}";

				sum += reportItem.TotalSum;
				row++;
			}
			maxRow = row > maxRow ? row : maxRow;
			return sum;
		}

		protected virtual double FillOrderColumns(string columnAddress, List<OptOrder> data, int row, ExcelWorksheet worksheet)
		{
			double sum = 0;
			foreach (var order in data)
			{
				var productFromOrder = order.OptOrderproducts.FirstOrDefault(x => x.ProductId == _helper.ProductId);
				var priceProductWithDiscount = productFromOrder.ProductPrice.Price *
									(100 - (order.EventDiscountPercent ?? 0 + order.PersonalDiscountPercent ?? 0 + order.OrderSizeDiscountPercent ?? 0))
									/ 100;
				double currentSum = productFromOrder.Amount * priceProductWithDiscount;
				worksheet.Cells[columnAddress + row].Value = $"Заказ №{order.ShortOrderId}, " +
					$"{productFromOrder.Amount} * " +
					$"{priceProductWithDiscount} = {currentSum}";

				sum += currentSum;
				row++;
			}
			maxRow = row > maxRow ? row : maxRow;
			return sum;
		}

		protected virtual void PrintHeader(ExcelWorksheet worksheet, int row)
		{
			worksheet.Cells["B" + row].Value = "СП";
			worksheet.Cells["C" + row].Value = "Магазины";
			worksheet.Cells["D" + row].Value = "Озон";
			worksheet.Cells["E" + row].Value = "ВБ";
			worksheet.Cells["F" + row].Value = "Остальное";

			worksheet.Column(1).Width = 15;
			worksheet.Column(2).Width = 30;
			worksheet.Column(3).Width = 30;
			worksheet.Column(4).Width = 45;
			worksheet.Column(5).Width = 55;
			worksheet.Column(6).Width = 50;
		}
	}
}
