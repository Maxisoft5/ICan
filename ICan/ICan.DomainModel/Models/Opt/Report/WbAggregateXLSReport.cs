using ICan.Common.Domain;
using ICan.Common.Utils;
using ICan.Business.Services;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;

namespace ICan.Common.Models.Opt.Report
{
	[Obsolete]
	public class WbAggregateXLSReport : WbAggregateReport
	{
		private readonly int _idCell = 2;
		private readonly int _amountCell;
		protected override int LastRow => _data.Tables[0].Rows.Count - 1;

		public WbAggregateXLSReport(ExcelWorksheet worksheet, DataSet data, int shopId, ReportKind reportKind, WbCity city,  IEnumerable<OptReportCriteriaGroup> criteriaGroups = null,
			 Dictionary<int, IdIsbnNameProductModel> productsWithIds  = null)
		: base(worksheet, data, shopId, reportKind, city, criteriaGroups, productsWithIds)
		{
			 _amountCell = ExcelUtil.CityDetails[city].AmountCell;
		}

		protected override string GetAmountCellValue(int i)
		{
			return _data.Tables[0].Rows[i][_amountCell].ToString();
		}

		protected override string GetIdCellValue(int rowId)
		{
			return _data.Tables[0].Rows[rowId][_idCell].ToString();
		}
	}
}
