using ICan.Common.Domain;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ICan.Common.Models.Opt.Report
{
	public class ConsolidatedWBReport
	{
		private readonly List<OptReport> reports = new List<OptReport>();
		private readonly List<ProductModel> products = new List<ProductModel>();
		private readonly DateTime PeriodDateFrom;
		
		public ConsolidatedWBReport(List<OptReport> reportsForConsolidate, List<ProductModel> productsForReport, DateTime periodDateFrom)
		{
			reports = reportsForConsolidate;
			products = productsForReport;
			PeriodDateFrom = periodDateFrom;
		}

		public byte[] GenerateReport()
		{
			byte[] result;
			using (var package = new ExcelPackage())
			{
				var worksheetWithWeek = package.Workbook.Worksheets.Add($"Недели");				
				FillInfoByWeeks(worksheetWithWeek);

				result = package.GetAsByteArray();
			}
			return result;
		}		

		private void FillInfoByWeeks(ExcelWorksheet worksheet)
		{
			SetDecor(worksheet);
			FillProductColumns(worksheet);
			var row = 2;
			var column = 3;
			for (var date = PeriodDateFrom; date <= DateTime.Now.Date; date = date.AddDays(7))
			{
				var dateTo = date.AddDays(6);
				worksheet.Column(column).Width = 12;
				worksheet.Cells[1, column].Value = date.Date.ToString("dd.MM") + " - " + dateTo.ToString("dd.MM");				
				var reportsByDates = reports.Where(x => x.ReportPeriodFrom >= date.Date && x.ReportPeriodTo <= dateTo.Date);
				row = 2;
				foreach (var product in products)
				{
					var soldAmount = reportsByDates.SelectMany(x => x.ReportItems).Where(x => x.ProductId == product.ProductId).Sum(x => x.Amount);
					worksheet.Cells[row, column].Value = soldAmount;
					row++;
				}
				column++;
			}
		}

		private void FillProductColumns(ExcelWorksheet worksheet)
		{
			var row = 2;
			foreach (var product in products)
			{
				worksheet.Cells[row, 1].Value = product.DisplayName;
				worksheet.Cells[row, 2].Value = product.ISBN;
				row++;
			}
		}

		private void SetDecor(ExcelWorksheet worksheet)
		{
			worksheet.Cells[1, 1].Value = "Наименование";
			worksheet.Cells[1, 2].Value = "Артикул";
			worksheet.Column(1).Width = 55;
			worksheet.Column(2).Width = 15;
			worksheet.Row(1).Height = 20;
			worksheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
			worksheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
			worksheet.Row(1).Style.Font.Bold = true;
			worksheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
		}
	}
}
