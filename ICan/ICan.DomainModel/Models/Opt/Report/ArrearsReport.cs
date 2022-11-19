using ICan.Common.Domain;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ICan.Common.Models.Opt.Report
{
	public class ArrearsReport
	{
		private int row = 1;
		private double TotalSum = 0;
		private decimal PaidSum = 0;
		private decimal Arrears = 0;
		private List<OptPrintOrder> printOrders = new List<OptPrintOrder>();
		private readonly string _hostName;

		public ArrearsReport(string hostName, List<OptPrintOrder> printOrdersForReport)
		{
			_hostName = hostName;
			printOrders = printOrdersForReport;
		}

		public byte[] GenerateReport()
		{
			byte[] result;
			using (var package = new ExcelPackage())
			{
				var worksheet = package.Workbook.Worksheets.Add($"Заказы печати");
				SetHeader(worksheet);
				FillBody(worksheet);
				FillFooter(worksheet);
				result = package.GetAsByteArray();
			}
			return result;
		}

		private void FillFooter(ExcelWorksheet worksheet)
		{
			row++;

			worksheet.Cells[row, 1].Value = "Итого:";
			worksheet.Cells[row, 1, row, 6].Style.Font.Bold = true;
			worksheet.Cells[row, 1, row, 6].Style.Numberformat.Format = "###,###,##0.00 ₽";
			worksheet.Cells[2, 3, row-2, 6].Style.Numberformat.Format = "###,###,##0.00";
			worksheet.Cells[2, 4, row-2, 4].Style.Numberformat.Format = "###,###,##0";
			worksheet.Cells[row, 3].Value = TotalSum;
			worksheet.Cells[row, 5].Value = PaidSum;
			worksheet.Cells[row, 6].Value = Arrears;			
		}

		private void FillBody(ExcelWorksheet worksheet)
		{
			foreach(var printOrder in printOrders)
			{
				var paidSum = printOrder.PrintOrderPayments.Sum(x => x.Amount);
				var arrears = (decimal)printOrder.OrderSum - paidSum;
				worksheet.Cells[row, 1].Formula = string.Format(@"HYPERLINK(""{0}"", ""{1}"")", $"{_hostName}/PrintOrder/Edit/{printOrder.PrintOrderId}",
					printOrder.PrintingHouseOrderNum);
				worksheet.Cells[row, 1].Style.Font.UnderLine = true;
				worksheet.Cells[row, 1].Style.Font.Color.SetColor(Color.Blue);
				worksheet.Cells[row, 2].Value = printOrder.OrderDate.ToShortDateString();
				worksheet.Cells[row, 3].Value = printOrder.OrderSum;
				worksheet.Cells[row, 4].Value = printOrder.Printing;
				worksheet.Cells[row, 5].Value = paidSum;
				worksheet.Cells[row, 6].Value = arrears;
				
				PaidSum += paidSum;
				TotalSum += printOrder.OrderSum;
				Arrears += arrears;
				row++;
			}
		}

		private void SetHeader(ExcelWorksheet worksheet)
		{
			worksheet.Cells[row, 1].Value = "Номер заказа в типографии";
			worksheet.Cells[row, 2].Value = "Дата";
			worksheet.Cells[row, 3].Value = "Сумма";
			worksheet.Cells[row, 4].Value = "Тираж";
			worksheet.Cells[row, 5].Value = "Оплаченная сумма";
			worksheet.Cells[row, 6].Value = "Задолженность";
			worksheet.Cells[row, 1, row, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
			worksheet.Cells[row, 1, row, 6].Style.Font.Bold = true;
			worksheet.Column(1).Width = 50;
			for (var i = 2; i <= 6; i++)
			{
				worksheet.Column(i).Width = 20;
			}
			worksheet.Column(2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
			row++;
		}
	}
}
