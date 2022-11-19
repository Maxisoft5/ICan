using ICan.Common.Domain;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Common.Models.Opt.Report
{
	public class ReadingCityXLSReport : Report
	{

		private readonly new int _firstRow = 1;

		private readonly int _idCell = 1;
		private readonly int _amountCell = 5;
		private readonly int _sumColmn = 6;

		public ReadingCityXLSReport(ExcelWorksheet worksheet, DataSet data, int shopId, ReportKind reportKind, IEnumerable<OptReportCriteriaGroup> criteriaGroups = null,
			 Dictionary<int, IdIsbnNameProductModel> productsWithIds = null)
			: base(worksheet, data, shopId, reportKind, criteriaGroups, productsWithIds)
		{
		}

		protected override int FirstRow => _firstRow;

		protected override int LastRow => _data.Tables[0].Rows.Count - 1;


		protected override string BookIdColumn => "F";

		protected override string BookAmountColumn => "I";
		protected override string TotalSumColumn => "K";

		public override void SetShopItems()
		{
			var list = new List<ShopItemInfo>();

			for (var i = FirstRow; i <= LastRow; i++)
			{
				var bookInfo = GetBookId(i.ToString());
				var bookId = bookInfo.Item1;
				var isbn = bookInfo.Item2;
				if (bookId == -1)
					continue;

				var amountCellValue = _data.Tables[0].Rows[i][_amountCell].ToString();
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
			for (var i = FirstRow; i <= LastRow; i++)
			{
				TotalSum += Double.Parse(_data.Tables[0].Rows[i][_sumColmn].ToString());
			}
			TotalSum = Math.Round(TotalSum, 2);
		}


		protected override Tuple<int, string> GetBookId(string rowId)
		{

			var cellValue = _data.Tables[0].Rows[int.Parse(rowId)][_idCell].ToString();
			if (string.IsNullOrWhiteSpace(cellValue))
				throw new Exception($"Невозможно определить isbn книги в ячейке {rowId}  {_idCell}");
			var isbn = cellValue.ToLower().Trim();
			var book = ProductsWithIds.FirstOrDefault(product => string.Equals(isbn, product.Value.ISBN.Trim(), StringComparison.InvariantCultureIgnoreCase));
			if (book.Equals(default(KeyValuePair<int, IdIsbnNameProductModel>)))
				throw new Exception($"Невозможно определить id по isbn {isbn} книги в ячейке  {rowId}  {_idCell}");
			return Tuple.Create<int, string>(book.Key, isbn);
		}

	}
}
