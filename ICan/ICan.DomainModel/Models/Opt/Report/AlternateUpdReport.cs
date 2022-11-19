using ICan.Common.Domain;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ICan.Common.Models.Opt.Report
{
	public class AlternateUpdReport : Report
	{
		private int _lastRow = -1;

		protected override string DateFormat => "d MMMM yyyy г.";
		protected override string DateCell => "Y1";
		protected override string ReportNumCell => "P1";

		protected override string BookIdColumn => "B";


		protected override string BookAmountColumn => "AA";
		protected override string TotalSumColumn => "BF";

		protected override string BookTotalSumColumn => "BF";

		public override bool CanSkipRowItems => true;


		public AlternateUpdReport(ExcelWorksheet worksheet, DataSet data, int shopId, ReportKind reportKind, IEnumerable<OptReportCriteriaGroup> criteriaGroups = null,
			 Dictionary<int, IdIsbnNameProductModel> productsWithIds = null)
			: base(worksheet, data, shopId, reportKind, criteriaGroups, productsWithIds) { }

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
							var cellValue   = _worksheet.Cells[i, 6].Value?.ToString().ToLower().Trim();
							if (!string.IsNullOrWhiteSpace(cellValue)
									&& cellValue.Contains("п/п"))

							{
								_firstRow = i + 3;
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
					var maxRow = _worksheet.Dimension?.Rows;
					for (var i = 1; i <= maxRow; i++)
					{
						var cellValue = _worksheet.Cells[i, 6].Value?.ToString();
						if (!string.IsNullOrWhiteSpace(cellValue)
								&& cellValue.ToLower().Trim().Contains("всего к оплате"))
						{
							_lastRow = i - 1;
							return _lastRow;
						}
					}
					throw new Exception("Невозможно найти последнюю строку с позициями в отчёте");
				}
				return _lastRow;
			}
		}


		protected override Tuple<int, string> GetBookId(string bookIdCell)
		{
			var cellValue = _worksheet.Cells[bookIdCell].Value?.ToString();
			if (string.IsNullOrWhiteSpace(cellValue))
				if (CanSkipRowItems)
					return new Tuple<int, string>(-1, "");
				else
					throw new Exception($"Невозможно определить isbn книги в ячейке {bookIdCell}");
			var serviceCode = cellValue.ToLower().Replace("-", "").Trim();
			var book = ProductsWithIds.FirstOrDefault(product => 
				string.Equals(serviceCode, product.Value.ISBN.Trim(), StringComparison.InvariantCultureIgnoreCase)
				
				);

			if (book.Equals(default(KeyValuePair<int, IdIsbnNameProductModel>))) {
				book = ProductsWithIds.FirstOrDefault(product =>
				   (!string.IsNullOrWhiteSpace(product.Value.ArticleNumber) && string.Equals(serviceCode, product.Value.ArticleNumber.Trim(), StringComparison.InvariantCultureIgnoreCase)));
			}

			if (book.Equals(default(KeyValuePair<int, IdIsbnNameProductModel>)))
				if (CanSkipRowItems)
					return new Tuple<int, string>(-1, "");
				else
					throw new Exception($"Невозможно определить id по isbn {serviceCode} книги в ячейке {bookIdCell}");
			return new Tuple<int, string>(book.Key, serviceCode);
		}

		protected override double GetTotalSumForProduct(int row)
		{
			var cellValue = _worksheet.Cells[BookTotalSumColumn + row].Value?.ToString();
			return double.TryParse(cellValue, out var totalSum) ? totalSum : 0;
		}
	}

}
