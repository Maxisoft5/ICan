using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;

namespace ICan.Common.Models.Opt.Report
{
	public class MyShopReport : Report
	{
		protected ReportWarning reportWarning;

		private string _bookIdColumn;

		private string _bookAmountColumn;

		private string _totalSumColumn;

		public MyShopReport( ExcelWorksheet worksheet, DataSet data, int shopId, ReportKind reportKind, IEnumerable<OptReportCriteriaGroup> criteriaGroups = null,
			 Dictionary<int, IdIsbnNameProductModel> productsWithIds  = null)
			: base( worksheet, data, shopId, reportKind, criteriaGroups, productsWithIds)
		{
			InitColumns();
			reportWarning = new ReportWarning();
			reportWarning.ShopId = shopId;
			DateFormat = GetCriteria(ReportCriteriaType.DateFormat)?.Information;
			DateCell = GetCriteria(ReportCriteriaType.Date)?.Address;

			var bookIdColumnFromDb = GetCriteria(ReportCriteriaType.BookId)?.Address;
			if (_bookIdColumn.Equals(bookIdColumnFromDb))
			{
				BookIdColumn = bookIdColumnFromDb;
			}
			else
			{
				BookIdColumn = _bookIdColumn;
				reportWarning.Fields.Add(new ReportWarningField(ReportCriteriaType.BookId.ToString(), bookIdColumnFromDb, _bookIdColumn));
			}

			var bookAmountColumnFromDb = GetCriteria(ReportCriteriaType.BookAmount)?.Address;
			if (_bookAmountColumn.Equals(bookAmountColumnFromDb))
			{
				BookAmountColumn = bookAmountColumnFromDb;
			}
			else
			{
				BookAmountColumn = _bookAmountColumn;
				reportWarning.Fields.Add(new ReportWarningField(ReportCriteriaType.BookAmount.ToString(), bookAmountColumnFromDb, _bookAmountColumn));
			}

			var totalSumColumnFromDb = GetCriteria(ReportCriteriaType.TotalSum)?.Address;
			if (_totalSumColumn.Equals(totalSumColumnFromDb))
			{
				TotalSumColumn = totalSumColumnFromDb;
			}
			else
			{
				TotalSumColumn = _totalSumColumn;
				reportWarning.Fields.Add(new ReportWarningField(ReportCriteriaType.TotalSum.ToString(), totalSumColumnFromDb, _totalSumColumn));
			}
		}

		public override void SetReportDate()
		{
			var cellValue = _worksheet.Cells[DateCell].Text;
			if (!string.IsNullOrWhiteSpace(cellValue))
			{
				var splittedInfo = cellValue.Split("-");
				cellValue = splittedInfo[splittedInfo.Length - 1].Trim();
			}

			SetDateByValue(cellValue);
		}

		protected override int FirstRow
		{
			get
			{
				for (var i = 1; i <= _worksheet.Dimension.Rows; i++)
				{
					var cellValue = _worksheet.Cells[i, 1].Value?.ToString();
					if (!string.IsNullOrWhiteSpace(cellValue)
							&& string.Equals(cellValue.ToLower().Trim(), "я могу"))

					{
						return ++i;
					}
				}
				throw new Exception("Невозможно найти первую строку с позициями в отчёте");
			}
		}

		protected override int LastRow
		{
			get
			{
				for (var i = FirstRow; i <= _worksheet.Dimension.Rows; i++)
				{
					var cellValue = _worksheet.Cells[i, 1].Value?.ToString();
					if (!string.IsNullOrWhiteSpace(cellValue)
							&& string.Equals(cellValue.ToLower().Trim(), "итого"))

					{
						return --i;
					}
				}
				throw new Exception("Невозможно найти последнюю строку с позициями в отчёте");
			}
		}

		private int _headerRow
		{
			get
			{
				var cellValue = "";
				for (var i = 1; i < _worksheet.Dimension.Rows; i++)
				{
					cellValue = _worksheet.Cells[i, 1].Text;
					if (!string.IsNullOrWhiteSpace(cellValue) && cellValue.Trim().ToLower().Equals("номенклатура"))
						return i;
				}
				throw new Exception("Невозможно определить номер строки, с которой начинается заголовок таблицы");
			}
		}

		private void InitColumns()
		{
			var cellValue = "";
			for (var i = 1; i < _worksheet.Dimension.Columns; i++)
			{
				cellValue = _worksheet.Cells[_headerRow, i].Text.Trim().ToLower();
				if (!string.IsNullOrWhiteSpace(cellValue))
				{
					if (cellValue.Equals("isbn"))
						_bookIdColumn = _worksheet.Cells[_headerRow, i].Address[0].ToString();
					if (cellValue.Equals("реализация (купля-продажа)"))
					{
						//Расход сумма
						_totalSumColumn = _worksheet.Cells[_headerRow, i].Address[0].ToString();
						//Расход количество
						_bookAmountColumn = _worksheet.Cells[_headerRow, i + 1].Address[0].ToString();
						break;
					}
				}
			}
		}
	}
}
