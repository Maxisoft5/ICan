using ICan.Common.Domain;
using ICan.Common.Utils;
using ICan.Business.Services;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Data;

namespace ICan.Common.Models.Opt.Report
{
	public class WbAggregateXLSXReport : WbAggregateReport
	{
		protected override int FirstRow => 3;
		protected override string BookIdColumn => "C";

		protected override string BookAmountColumn => ExcelUtil.CityDetails[City].AmountColumn;

		public WbAggregateXLSXReport(ExcelWorksheet worksheet, DataSet data, int shopId, ReportKind reportKind, WbCity city, IEnumerable<OptReportCriteriaGroup> criteriaGroups = null,
			 Dictionary<int, IdIsbnNameProductModel> productsWithIds  = null)
		: base(worksheet, data, shopId, reportKind, city, criteriaGroups, productsWithIds)
		{
		}
		protected override string GetAmountCellValue(int i)
		{
			var bookCell = BookAmountColumn + i;
			return _worksheet.Cells[bookCell].Value?.ToString();
		}
		protected override string GetIdCellValue(int rowId)
		{
			var bookCell = BookIdColumn + rowId;
			return _worksheet.Cells[bookCell].Value?.ToString();
		}
	}
}
