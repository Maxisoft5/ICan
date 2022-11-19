using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ICan.Common.Models.Opt.Report
{
	public class OzonReport : Report
	{
		private string _bookAmountColumn;

		private string _bookIdColumn;

		private new int _firstRow;
		protected override int FirstRow => _firstRow;

		private int _lastRow;
		protected override int LastRow => _lastRow;

		protected ReportWarning reportWarning;
		private string ComissionColumn { get; set; }


		public OzonReport(ExcelWorksheet worksheet, DataSet data, int shopId,
			ReportKind reportKind, IEnumerable<OptReportCriteriaGroup> criteriaGroups = null,
			 Dictionary<int, IdIsbnNameProductModel> productsWithIds = null) : base(worksheet, data, shopId, reportKind, criteriaGroups, productsWithIds)
		{
			InitColumns();
			InitRows();
			reportWarning = new ReportWarning();
			reportWarning.ShopId = shopId;
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

			BookTotalSumColumn = GetCriteria(ReportCriteriaType.ReportItemTotalSum)?.Address;
			TotalSumColumn = GetCriteria(ReportCriteriaType.TotalSum)?.Address;
			DateFormat = GetCriteria(ReportCriteriaType.DateFormat)?.Information;
			ComissionColumn = GetCriteria(ReportCriteriaType.Commision)?.Address;
			//!!!if (reportWarning.Fields.Count > 0)
			//{
			//	reportWarning.ShopName = _context.OptShop.FirstOrDefault(x => x.ShopId == reportWarning.ShopId).Name;
			//	warnings = reportWarning;
			//}
		}

		public override void SetReportDate()
		{
			DateCell = GetCriteria(ReportCriteriaType.Date)?.Address;
			var cellValue = _worksheet.Cells[DateCell].Text.Split(" ");
			var date = cellValue[cellValue.Length - 1];

			SetDateByValue(date);
		}

		public override void SetReportNum()
		{
			ReportNumCell = GetCriteria(ReportCriteriaType.ReportNumber)?.Address;
			if (ReportNumCell == null)
				return;
			var cellValue = _worksheet.Cells[ReportNumCell].Text.Split(" ");

			ReportNum = cellValue[cellValue.Length - 1];
		}

		public override void SetTotalSum()
		{
			for (var i = LastRow; i < _worksheet.Dimension.Rows; i++)
			{
				var cellValue = _worksheet.Cells[TotalSumColumn + i]?.Value?.ToString();
				if (!string.IsNullOrEmpty(cellValue) && double.TryParse(cellValue, out var sum))
				{
					TotalSum = sum;
					break;
				}
			}
		}

		protected override double GetTotalSumForProduct(int row)
		{
			var cellSumValue = _worksheet.Cells[BookTotalSumColumn + row].Value?.ToString();
			var cellComissionValue = _worksheet.Cells[ComissionColumn + row].Value?.ToString();

			double.TryParse(cellSumValue, out var sumValue);
			double.TryParse(cellComissionValue, out var comissionValue);

			return sumValue - comissionValue;
		}

		protected override Tuple<int, string> GetBookId(string bookIdCell)
		{
			var cellValue = _worksheet.Cells[bookIdCell].Value?.ToString();
			string bookName = "";
			if (string.IsNullOrWhiteSpace(cellValue))
			{
				bookIdCell = bookIdCell.Replace("F", "C");
				cellValue = _worksheet.Cells[bookIdCell].Value?.ToString();
				if (!string.IsNullOrWhiteSpace(cellValue))
				{
					var splittedCellValue = cellValue.Split("(");
					bookName = splittedCellValue[0].Trim();
					var notebook = ProductsWithIds.FirstOrDefault(product => product.Value.Name.Equals(bookName, StringComparison.InvariantCultureIgnoreCase));

					if (notebook.Equals(default(KeyValuePair<int, IdIsbnNameProductModel>)))
					{
						throw new UserException($"Невозможно определить isbn для \"{bookName}\" в ячейке {bookIdCell}");
					}
					else
					{
						cellValue = notebook.Value.ISBN;

					}
				}
				else
				{
					throw new UserException($"Невозможно определить название тетради в ячейке {bookIdCell}");
				}
			}

			var isbn = cellValue.ToLower().Replace("-", "").Trim();
			var book = ProductsWithIds.FirstOrDefault(product => string.Equals(isbn, product.Value.ISBN.Trim(), StringComparison.InvariantCultureIgnoreCase));

			if (book.Equals(default(KeyValuePair<int, IdIsbnNameProductModel>)) || string.IsNullOrWhiteSpace(book.Value.ISBN))
				if (CanSkipRowItems)
					return new Tuple<int, string>(-1, "");
				else
					throw new Exception($"Невозможно определить  id по isbn {isbn} книги в ячейке {bookIdCell}");
			return new Tuple<int, string>(book.Key, isbn);
		}

		private int rowHeader
		{
			get
			{
				var cellValue = "";
				for (var i = 1; i < _worksheet.Dimension.Rows; i++)
				{
					cellValue = _worksheet.Cells[i, 2].Text.Trim().ToLower();
					if (!string.IsNullOrWhiteSpace(cellValue) && cellValue.Equals("№ п/п"))
						return i;
				}
				throw new Exception("Невозможно определить номер строки, с которой начинается заголовок таблицы");
			}
		}

		private void InitRows()
		{
			var cellValue = "";
			for (var i = 1; i < _worksheet.Dimension.Rows; i++)
			{
				cellValue = _worksheet.Cells[i, 3]?.Value?.ToString().ToLower();
				if (!string.IsNullOrWhiteSpace(cellValue) && cellValue.Equals("товар"))
				{
					_firstRow = i + 2;
					break;
				}
			}
			for (var i = _firstRow; i < _worksheet.Dimension.Rows; i++)
			{
				cellValue = _worksheet.Cells[i, 3]?.Value?.ToString().ToLower();
				if (string.IsNullOrWhiteSpace(cellValue))
				{
					if (string.IsNullOrWhiteSpace(cellValue))
					{
						_lastRow = i - 1;
						return;
					}
				}
			}
			throw new Exception("Невозможно найти первую и/или последнюю строку");
		}

		private void InitColumns()
		{
			var cellValue = "";
			var cols = _worksheet.Dimension.Columns;
			var headerRow = rowHeader;
			for (var i = 1; i < cols; i++)
			{
				cellValue = _worksheet.Cells[headerRow, i].Text.Trim().ToLower();
				if (!string.IsNullOrWhiteSpace(cellValue))
				{
					if (cellValue.Equals("код товара продавца"))
						_bookIdColumn = _worksheet.Cells[headerRow, i].Address[0].ToString();
					if (cellValue.Equals("реализовано"))
					{
						for (var r = i; r < cols; r++)
						{
							cellValue = _worksheet.Cells[headerRow + 1, i].Text.Trim().ToLower();
							if (cellValue.Equals("цена"))
							{
								_bookAmountColumn = _worksheet.Cells[headerRow + 1, i + 2].Address[0].ToString();
								return;
							}
						}
					}
				}
			}
			throw new Exception("Невозможно найти некоторые столбцы(ISBN/Amount)");
		}
	}
}
