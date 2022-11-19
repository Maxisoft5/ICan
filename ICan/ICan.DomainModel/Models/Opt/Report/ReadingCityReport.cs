using ICan.Common.Domain;
using ICan.Common.Models.Enums;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ICan.Common.Models.Opt.Report
{
	public class ReadingCityReport : Report
	{
		protected override int FirstRow => 2;
		public ReadingCityReport(ExcelWorksheet worksheet,
			DataSet data,
			int shopId, ReportKind reportKind, IEnumerable<OptReportCriteriaGroup> criteriaGroups = null,
			 Dictionary<int, IdIsbnNameProductModel> productsWithIds  = null)
			: base(worksheet, data, shopId, reportKind, criteriaGroups, productsWithIds)
		{
			BookIdColumn = GetCriteria(ReportCriteriaType.BookId)?.Address;
			BookAmountColumn = GetCriteria(ReportCriteriaType.BookAmount)?.Address;
			TotalSumColumn = GetCriteria(ReportCriteriaType.TotalSum)?.Address;
		}

		public override void SetTotalSum()
		{
			var cellValues = _worksheet.Cells[TotalSumColumn + 2 + ":" + TotalSumColumn + LastRow].ToList();
			TotalSum = Math.Round(cellValues.Sum(t => double.Parse(t.Value.ToString())), 2);
		}
	}
}
