using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace ICan.Common.Models.Opt.Report
{
	public class WBReportByPeriod: WbReport
	{
		protected override int RowWithDate => 5;
		protected override string BookTotalSumColumn => "L";
		private readonly string PeriodFromAddress = "L7";
		private readonly string PeriodToAddress = "N7";
		protected override string PaidSumColumn => "M";
		public WBReportByPeriod(ExcelWorksheet worksheet, DataSet data, int shopId, ReportKind reportKind,
		 IEnumerable<OptReportCriteriaGroup> criteriaGroups = null,
			 Dictionary<int, IdIsbnNameProductModel> productsWithIds = null) : base(worksheet, data, shopId, reportKind, criteriaGroups, productsWithIds)
		{
			InitSumColumns();
			PeriodFromAddress = GetCriteria(ReportCriteriaType.PeriodFrom).Address;
			PeriodToAddress = GetCriteria(ReportCriteriaType.PeriodTo).Address;
		}
		protected override string DateCell
		{
			get
			{
				var cellWithDate = "";
				for (var i = 1; i < _worksheet.Dimension?.Columns; i++)
				{
					cellWithDate = _worksheet.Cells[RowWithDate, i].Value?.ToString().Split(" ")[2];
					if (!string.IsNullOrWhiteSpace(cellWithDate) && DateTime.TryParse(cellWithDate, out DateTime date))
						return _worksheet.Cells[RowWithDate, i].Address;
				}
				throw new UserException("Невозможно найти ячейку с датой отчёта");
			}
		}

		public override void SetReportDate()
		{
			var cellValue = _worksheet.Cells[DateCell].Text.Split(" ")[2];

			SetDateByValue(cellValue);
		}

		public override void SetReportNum()
		{
			var cellValue = _worksheet.Cells[DateCell].Text.Split(" ")[0];

			ReportNum = cellValue;
		}

		public override void SetPeriods()
		{
			if (DateTime.TryParse(_worksheet.Cells[PeriodFromAddress]?.Text, out var dateFrom))
				ReportPeriodFrom = dateFrom;
			if (DateTime.TryParse(_worksheet.Cells[PeriodToAddress]?.Text, out var dateTo))
				ReportPeriodTo = dateTo;
		}

		public override void SetTotalSum()
		{
			var paidSum = PaidSumColumn;
			var maxRow = LastRow + 1;

			for (var i = maxRow; i < _worksheet.Dimension.Rows; i++)
			{
				var value = _worksheet.Cells[i, 1]?.Value?.ToString().ToLower();
				if (!string.IsNullOrWhiteSpace(value) && value.Equals("3. расчеты с продавцом за текущий период"))
				{
					maxRow = i + 10;
					break;
				}
			}

			var cellValue = _worksheet.Cells[paidSum + maxRow]?.Value?.ToString().Replace(",", ".");
			if (!string.IsNullOrEmpty(cellValue))
			{
				double.TryParse(cellValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var sum);
				TotalSum = sum;
			}
		}
	}
}
