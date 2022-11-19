using ICan.Common.Domain;
using ICan.Common.Models.Exceptions;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ICan.Common.Models.Opt.Report
{
	public class LongIdFormatUpdReport : UpdReport
	{
		public LongIdFormatUpdReport(ExcelWorksheet worksheet, DataSet data, int shopId, ReportKind reportKind, IEnumerable<OptReportCriteriaGroup> criteriaGroups = null,
			 Dictionary<int, IdIsbnNameProductModel> productsWithIds = null)
			: base(worksheet, data, shopId, reportKind, criteriaGroups, productsWithIds) { }

		protected override Tuple<int, string> GetBookId(string bookIdCell)
		{
			var cellValue = _worksheet.Cells[bookIdCell].Value?.ToString();
			if (string.IsNullOrWhiteSpace(cellValue))
				if (CanSkipRowItems)
					return new Tuple<int, string>(-1, "");
				else
					throw new UserException($"Невозможно определить isbn книги в ячейке {bookIdCell}");
			var serviceCode = cellValue.ToLower().Replace("-", "").Trim();

			if (serviceCode.Contains("9785604001905"))
			{
				var product =
				ProductsWithIds.First(pr => pr.Value.ISBN.Equals("9785604501092"));
				return new Tuple<int, string>(product.Key, serviceCode);
			}

			if (serviceCode.Length > 13)
			{
				serviceCode = serviceCode.Substring(2);// 129785604001912 => 9785604001912
			}
			var book = ProductsWithIds.FirstOrDefault(product =>
			string.Equals(serviceCode, product.Value.ISBN.Trim(), StringComparison.InvariantCultureIgnoreCase)

			);
			if (book.Equals(default(KeyValuePair<int, IdIsbnNameProductModel>)))
			{
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
	}
}
