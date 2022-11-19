using ICan.Common.Domain;
using ICan.Common.Models.Exceptions;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace ICan.Common.Models.Opt.Report
{

	public class LeonardoAggReport : Report
	{
		protected string LocalShopNameCell => "A2";
		public LeonardoAggReport(ExcelWorksheet worksheet, DataSet data, int shopId, ReportKind reportKind, IEnumerable<OptReportCriteriaGroup> criteriaGroups = null, Dictionary<int, IdIsbnNameProductModel> productsWithIds = null)
			: base(worksheet, null, shopId, reportKind, criteriaGroups, productsWithIds) { }

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
									&& string.Equals(cellValue.ToLower().Trim(), "товар"))
							{
								//учитываем пустую строчку между шапкой и списком
								_firstRow = i + 2;
								return _firstRow;
							}
						}
						throw new UserException("Невозможно найти первую строку с позициями в отчёте");
					}
					return _firstRow;
				}
			}
		}

		public override bool CanSkipRowItems => true;

		protected override int LastRow => _worksheet.Dimension.End.Row > 1 ? _worksheet.Dimension.End.Row : 1;

		protected override string BookIdColumn => "C";

		protected override string BookAmountColumn => "D";

		public string LocalShopName { get; set; }

		public string TabName { get; set; }


		protected override Tuple<int, string> GetBookId(string bookIdCell)
		{
			var cellValue = _worksheet.Cells[bookIdCell].Value?.ToString();
			if (string.IsNullOrWhiteSpace(cellValue))
				throw new Exception($"Невозможно определить isbn книги в ячейке {bookIdCell} во вкладке {TabName}");
			var isbn = cellValue.Trim();
			var book = ProductsWithIds.FirstOrDefault(product => string.Equals(isbn, product.Value.ISBN.Trim(), StringComparison.InvariantCultureIgnoreCase));
			if (book.Equals(default(KeyValuePair<int, IdIsbnNameProductModel>)))
				throw new Exception($"Невозможно определить id по isbn {isbn} книги в ячейке {bookIdCell}");
			return new Tuple<int, string>(book.Key, isbn);
		}
		public void SetLocalShopName()
		{
			LocalShopName = _worksheet.Cells[LocalShopNameCell].Value.ToString();
		}
		public void SetTabName()
		{
			TabName = _worksheet.Name;
		}

		public override void SetShopItems()
		{
			var list = new List<ShopItemInfo>();

			for (var i = FirstRow; i <= LastRow; i++)
			{
				var bookIdCell = BookIdColumn + i;

				var bookInfo = GetBookId(bookIdCell);
				var isbn = bookInfo.Item2; //ISBN
				if (bookInfo.Item1 == -1) // key 
					continue;
				var bookAmountCell = BookAmountColumn + i;
				var amountCellValue = _worksheet.Cells[bookAmountCell].Value?.ToString();
				if (string.IsNullOrWhiteSpace(amountCellValue)
					|| !int.TryParse(amountCellValue, out var amount))
				{
					if (CanSkipRowItems)
						continue;
					else
						throw new Exception($"Невозможно определить количество проданных книг в строке {i} на листе {TabName}");
				}

				list.Add(new ShopItemInfo { NoteBookId = bookInfo.Item1, Amount = amount, ISBN = isbn });
			}

			ReportItems = list;
		}
	}
}
