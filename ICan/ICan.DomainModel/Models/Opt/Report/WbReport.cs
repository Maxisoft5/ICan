using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;


namespace ICan.Common.Models.Opt.Report
{
	public class WbReport : Report
	{
		private int _lastRow = -1;
		protected ReportWarning reportWarning;
		private string _bookIdColumn = "";
		private string _bookAmountColumn = "";
		private string _totalSumColumn = "";
		protected virtual int RowWithDate { get; set; } = 10;
		protected virtual string PaidSumColumn
		{
			get
			{
				if (Year == 2019)
					return "V";
				return "S";
			}
		}
		private string _bookTotalSumColumn;
		protected override string BookTotalSumColumn 
		{
			get
			{
				if (Year == 2019)
					return "V";
				return _bookTotalSumColumn;
			}
		}

		public WbReport(ExcelWorksheet worksheet, DataSet data, int shopId, ReportKind reportKind,
		 IEnumerable<OptReportCriteriaGroup> criteriaGroups = null,
			 Dictionary<int, IdIsbnNameProductModel> productsWithIds = null)
			: base(worksheet, data, shopId, reportKind, criteriaGroups, productsWithIds)
		{
			InitSumColumns();
			reportWarning = new ReportWarning();
			reportWarning.ShopId = shopId;

			DateFormat = GetCriteria(ReportCriteriaType.DateFormat)?.Information;

			var totalSumFromBd = GetCriteria(ReportCriteriaType.TotalSum)?.Address;
			if (_totalSumColumn.Equals(totalSumFromBd))
			{
				TotalSumColumn = totalSumFromBd;
			}
			else
			{
				TotalSumColumn = _totalSumColumn;
				reportWarning.Fields.Add(new ReportWarningField(ReportCriteriaType.TotalSum.ToString(), totalSumFromBd, _totalSumColumn));
			}

			var bookAmountColumnFromBd = GetCriteria(ReportCriteriaType.BookAmount)?.Address;
			if (_bookAmountColumn.Equals(bookAmountColumnFromBd))
			{
				BookAmountColumn = bookAmountColumnFromBd;
			}
			else
			{
				BookAmountColumn = _bookAmountColumn;
				reportWarning.Fields.Add(new ReportWarningField(ReportCriteriaType.BookAmount.ToString(), bookAmountColumnFromBd, _bookAmountColumn));
			}

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

			_bookTotalSumColumn = GetCriteria(ReportCriteriaType.ReportItemTotalSum)?.Address;

			if (reportWarning.Fields.Count > 0)
			{
				Warning = reportWarning;
			}
		}

		public override void SetReportDate()
		{
			var cellValue = _worksheet.Cells[DateCell].Text;

			SetDateByValue(cellValue);
		}

		public override void SetTotalSum()
		{
			var paidSum = PaidSumColumn;
			var maxRow = LastRow + 1;

			for (var i = maxRow; i < _worksheet.Dimension.Rows; i++)
			{
				var value = _worksheet.Cells[i, 1]?.Value?.ToString().ToLower();
				if (!string.IsNullOrWhiteSpace(value) && value.Equals("3. расчеты с принципалом за текущий период"))
				{
					maxRow = i + 11;
					break;
				}
			}

			var cellValue = _worksheet.Cells[paidSum + maxRow]?.Value?.ToString().Replace(",", ".");
			if (!string.IsNullOrEmpty(cellValue))
			{		
				double.TryParse(cellValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var sum);
				TotalSum = sum;
			}
		}

		protected override string ReportNumCell
		{
			get
			{
				var cellValue = _worksheet.Cells[8, 12].Value?.ToString();
				if (!string.IsNullOrWhiteSpace(cellValue))
					return _worksheet.Cells[8, 12].Address;
				return null;
			}
		}

		protected override int FirstRow
		{
			get
			{
				{
					if (_firstRow == -1)
					{
						var maxRow = _worksheet.Dimension?.Rows;
						for (var i = 1; i <= maxRow; i++)
						{
							var cellValue = _worksheet.Cells[i, 1].Value?.ToString();
							if (!string.IsNullOrWhiteSpace(cellValue)
									&& string.Equals(cellValue.ToLower().Trim(), "реализовано товара за текущий период"))

							{
								_firstRow = i + 4;
								return _firstRow;
							}
						}
						throw new UserException("Невозможно найти первую строку с позициями в отчёте");
					}
					return _firstRow;
				}
			}
		}

		protected override int LastRow
		{
			get
			{
				if (_lastRow == -1)
				{
					var maxRow = _worksheet.Dimension?.Rows;
					for (var i = 1; i <= maxRow; i++)
					{
						var cellValue = _worksheet.Cells[i, 1].Value?.ToString();
						if (!string.IsNullOrWhiteSpace(cellValue)
								&& string.Equals(cellValue.ToLower().Trim(), "2. возвраты реализованного товара от третьих лиц"))

						{
							_lastRow = i - 3;
							return _lastRow;
						}
					}
					throw new UserException("Невозможно найти последнюю строку с позициями в отчёте");
				}
				return _lastRow;
			}
		}

		protected override string DateCell
		{
			get
			{				
				var cellWithDate = "";
				for (var i = 1; i < _worksheet.Dimension?.Columns; i++)
				{
					cellWithDate = _worksheet.Cells[RowWithDate, i].Value?.ToString();
					if (!string.IsNullOrWhiteSpace(cellWithDate) && DateTime.TryParse(cellWithDate, out DateTime date))
						return _worksheet.Cells[RowWithDate, i].Address;
				}
				throw new UserException("Невозможно найти ячейку с датой отчёта");
			}
		}

		protected override Tuple<int, string> GetBookId(string bookIdCell)
		{
			var cellValue = _worksheet.Cells[bookIdCell].Value?.ToString();
			if (string.IsNullOrWhiteSpace(cellValue))
				throw new UserException($"Невозможно определить isbn книги в ячейке {bookIdCell}");
			var isbn = cellValue.Substring(2).ToLower().Trim();
			var book = ProductsWithIds.FirstOrDefault(product => string.Equals(isbn, product.Value.ISBN.Trim(), StringComparison.InvariantCultureIgnoreCase));

			if (book.Equals(default(KeyValuePair<int, IdIsbnNameProductModel>)) || string.IsNullOrWhiteSpace(book.Value.ISBN))
				throw new UserException($"Невозможно определить  id по isbn {isbn} книги в ячейке {bookIdCell}");
			return Tuple.Create<int, string>(book.Key, isbn);
		}

		protected override double GetTotalSumForProduct(int row)
		{
			var cellValue = _worksheet.Cells[BookTotalSumColumn + row].Value?.ToString();
			return double.TryParse(cellValue, out var totalSum) ? totalSum : 0;
		}

		protected void InitSumColumns()
		{
			var rowHeader = 24;
			var cellValue = "";
			for (var i = 1; i <= _worksheet.Dimension?.Rows; i++)
			{
				cellValue = _worksheet.Cells[i, 1].Value?.ToString().ToLower();
				if (!string.IsNullOrWhiteSpace(cellValue))
				{
					if (cellValue.Equals("№"))
					{
						rowHeader = i;
						break;
					}						
				}
			}
			
			for (var i = 1; i <= _worksheet.Dimension?.Columns; i++)
			{
				cellValue = _worksheet.Cells[rowHeader, i].Value?.ToString().ToLower();
				if (!string.IsNullOrWhiteSpace(cellValue))
				{
					if (cellValue.Equals("артикул"))
						_bookIdColumn = _worksheet.Cells[rowHeader, i].Address[0].ToString();
					if (cellValue.Equals("кол-во"))
						_bookAmountColumn = _worksheet.Cells[rowHeader, i].Address[0].ToString();
					if (cellValue.Equals("скидка постоянного покупателя (спп)"))
					{
						_totalSumColumn = _worksheet.Cells[rowHeader, i + 1].Address[0].ToString();
						break;
					}
				}
			}
		}
	}
}
