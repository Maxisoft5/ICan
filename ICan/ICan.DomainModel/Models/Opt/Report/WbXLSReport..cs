using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;


namespace ICan.Common.Models.Opt.Report
{
	public class WbXLSReport : Report
	{
		private int _lastRow = -1;
		private readonly int _dateRow = 8;
		private readonly int _dateColNew = 21;
		private readonly int _dateColOld = 20;
		private readonly int _totalSumCellNew = 14;
		private readonly int _totalSumCellOld = 13;

		private readonly int _paidSumCellNew = 21;
		private readonly int _paidSumCellOld = 20;


		private bool oldFormat = true;

		protected override string DateFormat => "yyyy-MM-dd";
		protected override string DateCell => "V9";
		public WbXLSReport(ExcelWorksheet worksheet, DataSet data, int shopId, ReportKind reportKind)
			: base(worksheet, data, shopId, reportKind) { }

		protected override int FirstRow
		{
			get
			{
				{
					if (_firstRow == -1)
					{
						//var maxRow = _worksheet.Dimension?.Rows;
						for (var i = 0; i < _data.Tables[0].Rows.Count; i++)
						{
							var cellValue = _data.Tables[0].Rows[i][0]?.ToString();
							if (!string.IsNullOrWhiteSpace(cellValue)
									&& string.Equals(cellValue.ToLower().Trim(), "реализовано комиссионного товара за текущий период"))

							{
								_firstRow = i + 4;
								return _firstRow;
							}

						}
						throw new Exception("Невозможно найти первую строку с позициями в отчёте");
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
					for (var i = 0; i < _data.Tables[0].Rows.Count; i++)
					{
						var cellValue = _data.Tables[0].Rows[i][0]?.ToString();

						if (!string.IsNullOrWhiteSpace(cellValue)
								&& string.Equals(cellValue.ToLower().Trim(), "2. возвраты реализованного комиссионного товара от третьих лиц"))

						{
							_lastRow = i - 3;
							return _lastRow;
						}

					}
					throw new Exception("Невозможно найти последнюю строку с позициями в отчёте");
				}
				return _lastRow;
			}
		}

		protected override string BookIdColumn => "F";

		protected override string BookAmountColumn => "I";
		protected override string TotalSumColumn => "K";

		public override void SetReportDate()
		{
			CultureInfo ru = new CultureInfo("ru-RU");
			var cellValue = _data.Tables[0].Rows[_dateRow][_dateColNew]?.ToString();

			if (!DateTime.TryParseExact(cellValue, DateFormat,
					ru,
				 DateTimeStyles.None, out var reportDate))
			{
				cellValue = _data.Tables[0].Rows[_dateRow][_dateColOld]?.ToString();
				oldFormat = true;
				if (!DateTime.TryParseExact(cellValue, DateFormat,
						ru,
					 DateTimeStyles.None, out reportDate))
				{
					throw new Exception("Невозможно определить дату отчёта");
				}
			}

			Month = reportDate.Month;
			Year = reportDate.Year;
		}

		public override void SetShopItems()
		{
			var list = new List<ShopItemInfo>();
			var _amountCell = 8;
			for (var i = FirstRow; i <= LastRow; i++)
			{

				var bookInfo = GetBookId(i.ToString());
				var bookId = bookInfo.Item1;
				var isbn = bookInfo.Item2;
				if (bookId == -1)
					continue;

				var amountCellValue = _data.Tables[0].Rows[i][_amountCell]?.ToString();
				if (string.IsNullOrWhiteSpace(amountCellValue)
					|| !int.TryParse(amountCellValue, out var amount))
				{
					if (CanSkipRowItems)
						continue;
					else
						throw new Exception($"Невозможно определить количество проданных книг в строке {i}");
				}

				list.Add(new ShopItemInfo { NoteBookId = bookId, Amount = amount, ISBN = isbn });
			}

			ReportItems = list.GroupBy(t => t.NoteBookId)
			   .Select(t => new ShopItemInfo { NoteBookId = t.Key, Amount = t.Sum(q => q.Amount) })
			   .ToList();
		}

		public override void SetTotalSum()
		{
			var totalSumCol = oldFormat ? _totalSumCellOld : _totalSumCellNew;

			var maxRow = LastRow + 1;
			var cellValue = _data.Tables[0].Rows[maxRow][totalSumCol]?.ToString();
			if (!string.IsNullOrEmpty(cellValue) && double.TryParse(cellValue, out var sum))
				TotalSum = sum;
		}

		public override void SetPaidSum()
		{
			var paidSum = oldFormat ? _paidSumCellOld : _paidSumCellNew;
			var maxRow = LastRow + 1;
			var cellValue = _data.Tables[0].Rows[maxRow][paidSum]?.ToString();
			if (!string.IsNullOrEmpty(cellValue) && double.TryParse(cellValue, out var sum))
				PaidSum = sum;
		}


		protected override Tuple<int, string> GetBookId(string rowId)
		{
			var _idCell = 5;
			var cellValue = _data.Tables[0].Rows[int.Parse(rowId)][_idCell]?.ToString();
			if (string.IsNullOrWhiteSpace(cellValue))
				throw new Exception($"Невозможно определить isbn книги в ячейке {rowId}  {_idCell}");
			var isbn = cellValue.Substring(2).ToLower().Trim();
			var book = ProductsWithIds.FirstOrDefault(product => string.Equals(isbn, product.Value.ISBN.Trim(), StringComparison.InvariantCultureIgnoreCase));

			if (book.Equals(default(KeyValuePair<int, IdIsbnNameProductModel>)) || string.IsNullOrWhiteSpace(book.Value.ISBN))
				throw new Exception($"Невозможно определить id по isbn {isbn} книги в ячейке {rowId}  {_idCell}");
			return Tuple.Create<int, string>(book.Key, isbn);
		}

	}
}
