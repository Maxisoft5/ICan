using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Linq;

namespace ICan.Common.Models.Opt.Report
{
	public class PrimerReportQty: PrimerReport
	{
		public PrimerReportQty(PrimerReportHelper helper) : base(helper) { }

		protected override List<double> PrintBody(int row, int month, ExcelWorksheet worksheet)
		{
			var sums = new List<double>();
			row++;

			sums.Add(FillOrderColumns("B", _helper.SPOrders.Where(x => x.DoneDate.Value.Month == month).ToList(), row, worksheet));
			sums.Add(FillOrderColumns("C", _helper.ShopOrders.Where(x => x.DoneDate.Value.Month == month).ToList(), row, worksheet));
			
			sums.Add(FillUpdColumns("D", _helper.OzonReportItems.Where(x => x.Report.ReportMonth == month && x.ProductId == _helper.ProductId).ToList(), row, worksheet));
			sums.Add(FillUpdColumns("E", _helper.WBReportItems.Where(x => x.Report.ReportMonth == month && x.ProductId == _helper.ProductId).ToList(), row, worksheet));
			sums.Add(FillUpdColumns("F", _helper.UPDReportItems.Where(x => x.Report.ReportMonth == month && x.ProductId == _helper.ProductId).ToList(), row, worksheet));
			
			sums.Add(FillWarehouseColumns("G", _helper.whJournalItems.Where(x => x.DateAdd.Month == month 
				&& x.WarehouseActionTypeId == (int)WarehouseActionType.Marketing).ToList(), row, worksheet));
			sums.Add(FillWarehouseColumns("H", _helper.whJournalItems.Where(x => x.DateAdd.Month == month
				&& x.WarehouseActionTypeId == (int)WarehouseActionType.Returning).ToList(), row, worksheet));
			return sums;
		}

		protected override double FillOrderColumns(string columnAddress, List<OptOrder> data, int row, ExcelWorksheet worksheet)
		{
			double sum = 0;
			foreach (var order in data)
			{
				var productFromOrder = order.OptOrderproducts.FirstOrDefault(x => x.ProductId == _helper.ProductId);
				worksheet.Cells[columnAddress + row].Value = $"Заказ №{order.ShortOrderId}, " +
					$"{productFromOrder.Amount} шт";

				sum += productFromOrder.Amount;
				row++;
			}
			maxRow = row > maxRow ? row : maxRow;
			return sum;
		}
	 
		private double FillWarehouseColumns(string columnAddress, List<OptWarehouse> data, int row, ExcelWorksheet worksheet)
		{
			double sum = data.SelectMany(x => x.WarehouseItems).Where(x => x.ProductId == _helper.ProductId).Sum(x => x.Amount);
			
			worksheet.Cells[columnAddress + row].Value = sum;
		
			row++;
			maxRow = row > maxRow ? row : maxRow;
			return sum;
		}

		protected override double FillUpdColumns(string columnAddress, List<OptReportitem> data, int row, ExcelWorksheet worksheet)
		{
			double sum = 0;
			foreach (var reportItem in data)
			{
				worksheet.Cells[columnAddress + row].Value = $"Отчёт №{reportItem.Report.ReportNum} {reportItem.Report.ReportDate.Value.Date.ToShortDateString()}, " +
					$"Кол-во {reportItem.Amount}";

				sum += reportItem.Amount;
				row++;
			}
			maxRow = row > maxRow ? row : maxRow;
			return sum;
		}

		protected override void PrintHeader(ExcelWorksheet worksheet, int row)
		{
			worksheet.Cells["B" + row].Value = "СП";
			worksheet.Cells["C" + row].Value = "Магазины";
			worksheet.Cells["D" + row].Value = "Озон";
			worksheet.Cells["E" + row].Value = "ВБ";
			worksheet.Cells["F" + row].Value = "УПД";
			worksheet.Cells["G" + row].Value = "Маркетинг";
			worksheet.Cells["H" + row].Value = "Возврат";

			worksheet.Column(1).Width = 15;
			worksheet.Column(2).Width = 30;
			worksheet.Column(3).Width = 30;
			worksheet.Column(4).Width = 30;
			worksheet.Column(5).Width = 30;
			worksheet.Column(6).Width = 15;
			worksheet.Column(7).Width = 15;
			worksheet.Column(7).Width = 15;
		}

		protected override void PrintFooter(List<double> sums, ExcelWorksheet worksheet)
		{
			worksheet.Cells["A" + maxRow].Value = $"Итого: {sums.Sum(x => x) - sums[6]*2 }";
			worksheet.Cells["B" + maxRow].Value = sums[0];
			worksheet.Cells["C" + maxRow].Value = sums[1];
			worksheet.Cells["D" + maxRow].Value = sums[2];
			worksheet.Cells["E" + maxRow].Value = sums[3];
			worksheet.Cells["F" + maxRow].Value = sums[4];
			worksheet.Cells["G" + maxRow].Value = sums[5];
			worksheet.Cells["H" + maxRow].Value = sums[6];
			worksheet.Cells[maxRow, 1, maxRow, 8].Style.Font.Bold = true;
		}
	}
}
