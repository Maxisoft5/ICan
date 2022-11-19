using ICan.Common.Domain;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Common.Models.Opt.Report
{
	public abstract class WbAggregateReport : Report
	{
		public readonly WbCity City;
		protected override int FirstRow => 2;

		public WbAggregateReport(ExcelWorksheet worksheet, DataSet data, int shopId, ReportKind reportKind, WbCity city, IEnumerable<OptReportCriteriaGroup> criteriaGroups = null,
			 Dictionary<int, IdIsbnNameProductModel> productsWithIds = null)
			: base(worksheet, data, shopId, reportKind, criteriaGroups, productsWithIds)
		{
			City = city;
		}

		public override void SetShopItems()
		{
			var list = new List<ShopItemInfo>();

			for (var i = FirstRow; i <= LastRow; i++)
			{
				var bookInfo = GetBookId(i);
				var bookId = bookInfo.Item1;
				var isbn = bookInfo.Item2;
				if (bookId == -1)
					continue;

				var amountCellValue = GetAmountCellValue(i);
				if (!string.IsNullOrWhiteSpace(amountCellValue)
					&& int.TryParse(amountCellValue, out var amount))
				{
					list.Add(new ShopItemInfo { NoteBookId = bookId, Amount = amount, ISBN = isbn });
				}
			}

			ReportItems = list.GroupBy(t => t.NoteBookId)
			   .Select(t => new ShopItemInfo { NoteBookId = t.Key, Amount = t.Sum(q => q.Amount) })
			   .ToList();
		}


		protected virtual Tuple<int, string> GetBookId(int rowId)
		{
			var idCellValue = GetIdCellValue(rowId);
			if (string.IsNullOrWhiteSpace(idCellValue))
				throw new Exception($"Невозможно определить isbn книги в строке {rowId} ");
			var isbn = idCellValue.Substring(2).ToLower().Trim();
			var book = ProductsWithIds.FirstOrDefault(t => string.Equals(isbn, t.Value.ISBN.Trim(), StringComparison.InvariantCultureIgnoreCase));
			if (book.Equals(default(KeyValuePair<int, IdIsbnNameProductModel>)))
				throw new Exception($"Невозможно определить id по isbn {isbn} книги в строке {rowId}");
			return Tuple.Create<int, string>(book.Key, isbn);
		}

		protected abstract string GetAmountCellValue(int i);
		protected abstract string GetIdCellValue(int rowId);
	}
}
