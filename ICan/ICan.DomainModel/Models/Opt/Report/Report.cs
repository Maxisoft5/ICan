using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using ICan.Common.Domain;

namespace ICan.Common.Models.Opt.Report
{
    public class Report
    {
        #region private static


        private static readonly int UpdVirtColumn = 3;


        private static readonly string UpdBeforeVirtText = "Иные сведения об отгрузке, передаче";
        private static readonly string UpdVirtText = "вирт";
        #endregion

        public ReportWarning Warning { get; protected set; } = new ReportWarning();

        protected CultureInfo ru = new CultureInfo("ru-RU");
        protected int _firstRow = -1;
        protected readonly DataSet _data;
        protected readonly IEnumerable<OptReportCriteriaGroup> _criteriaGroups;
        protected Dictionary<int, IdIsbnNameProductModel> ProductsWithIds { get; set; }
        protected readonly ExcelWorksheet _worksheet;
        protected readonly ILogger _logger;
        protected virtual string DateCell { get; set; }
        protected virtual string DateFormat { get; set; }
        protected virtual string ReportNumCell { get; set; }
        protected virtual int FirstRow => 1;
        protected virtual int LastRow => _worksheet?.Dimension.Rows ?? _data?.Tables[0].Rows?.Count ?? -1;
        protected virtual string BookIdColumn { get; set; }
        protected virtual string BookAmountColumn { get; set; }
        protected virtual string TotalSumColumn { get; set; }
        protected virtual string BookTotalSumColumn { get; set; }

        public virtual string ReportNum { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime? ReportDate { get; set; }
        public double TotalSum { get; set; }
        public double? PaidSum { get; set; }
        public bool IgnoreUpd { get; set; }
        public virtual bool CanSkipRowItems => false;
        public virtual bool UnknownShop => ShopId == (int)ShopType.None;
        public List<ShopItemInfo> ReportItems { get; set; } = new List<ShopItemInfo>();

        public static readonly int[] ShopWithoutDates = { (int)ShopType.Auсhan, (int)ShopType.ReadingCity };
        public int ShopId { get; set; }
        public int ReportKindId { get; set; }
        public bool NeedUpload { get; set; } = true;
        public bool IsVirtual { get; set; }
        public string UpdShopName { get; set; }
        public DateTime? ReportPeriodFrom { get; set; }
        public DateTime? ReportPeriodTo { get; set; }

        public Report() { }

        public Report(ExcelWorksheet worksheet,
             DataSet data, int shopId, ReportKind reportKind, IEnumerable<OptReportCriteriaGroup> criteriaGroups = null,
             Dictionary<int, IdIsbnNameProductModel> productsWithIds = null)
        {
            _worksheet = worksheet;
            ShopId = shopId;
            ReportKindId = (int)reportKind;
            _data = data;
            _criteriaGroups = criteriaGroups;
            ProductsWithIds = productsWithIds;
        }

        public void SetVirtAttr()
        {
            if (ReportKindId != (int)ReportKind.UPD || _worksheet == null)
                return;

            var virtrow = FindBeforeVirtRow() + 1;
            if (!virtrow.HasValue)
            {
                _logger.LogWarning("Не смогли определить ячейку с иными сведениями об УПД");
                return;
            }
            var cellValue = _worksheet.Cells[virtrow.Value, UpdVirtColumn].Value?.ToString();
            IsVirtual = !string.IsNullOrWhiteSpace(cellValue) && cellValue.Contains(UpdVirtText);
        }

        public static Report Empty()
        {
            return new Report { NeedUpload = false };
        }


        public bool NoDateInReport =>
             ShopWithoutDates.Contains(ShopId)
                            && (ReportKindId != (int)ReportKind.UPD);

        public static ShopType[] ShopsWithPaidSum = new ShopType[] { ShopType.WB };

        public virtual void SetReportDate()
        {
            if (NoDateInReport)
                return;
            var cellValue = _worksheet.Cells[DateCell].Text;
            SetDateByValue(cellValue);
        }

        public virtual void SetReportNum()
        {
            if (ReportNumCell == null)
                return;
            var cellValue = _worksheet.Cells[ReportNumCell].Text;
            ReportNum = cellValue;
        }

        protected virtual Tuple<int, string> GetBookId(string bookIdCell)
        {
            var cellValue = _worksheet.Cells[bookIdCell].Value?.ToString();
            if (string.IsNullOrWhiteSpace(cellValue))
                if (CanSkipRowItems)
                    return new Tuple<int, string>(-1, "");
                else
                    throw new Exception($"Невозможно определить isbn книги в ячейке {bookIdCell}");

            var isbn = cellValue.ToLower().Replace("-", "").Trim();
            var book = ProductsWithIds.FirstOrDefault(t => string.Equals(isbn, t.Value.ISBN.Trim(), StringComparison.InvariantCultureIgnoreCase));
            if (book.Equals(default(KeyValuePair<int, IdIsbnNameProductModel>)))
                if (CanSkipRowItems)
                    return new Tuple<int, string>(-1, "");
                else
                    throw new Exception($"Невозможно определить  id по isbn {isbn} книги в ячейке {bookIdCell}");
            return new Tuple<int, string>(book.Key, isbn);
        }


        public virtual void SetShopItems()
        {
            var list = new List<ShopItemInfo>();

            for (var i = FirstRow; i <= LastRow; i++)
            {
                var bookIdCell = BookIdColumn + i;
                var bookInfo = GetBookId(bookIdCell);
                var bookId = bookInfo.Item1;
                var isbn = bookInfo.Item2;
                if (bookId == -1)
                    continue;
                var bookAmountCell = BookAmountColumn + i;
                var amountCellValue = _worksheet.Cells[bookAmountCell].Value?.ToString();
                if (string.IsNullOrWhiteSpace(amountCellValue)
                    || !int.TryParse(amountCellValue, out var amount))
                {
                    if (CanSkipRowItems)
                        continue;
                    else
                        throw new UserException($"Невозможно определить количество проданных книг в строке {i}");
                }

                var totalSum = GetTotalSumForProduct(i);

                list.Add(new ShopItemInfo { NoteBookId = bookId, Amount = amount, ISBN = isbn, TotalSum = totalSum });
            }

            ReportItems = list.GroupBy(t => t.NoteBookId)
               .Select(t => new ShopItemInfo { NoteBookId = t.Key, Amount = t.Sum(q => q.Amount), TotalSum = t.Sum(t => t.TotalSum) })
               .ToList();
        }

        protected virtual double GetTotalSumForProduct(int row)
        {
            return 0;
        }

        public virtual void SetTotalSum()
        {
            var maxRow = LastRow + 1;
            var cellValue = _worksheet.Cells[TotalSumColumn + maxRow]?.Value?.ToString();
            if (!string.IsNullOrEmpty(cellValue) && double.TryParse(cellValue, out var sum))
                TotalSum = sum;
        }

        public virtual void SetPaidSum()
        {
        }

        protected OptReportCriteria GetCriteria(ReportCriteriaType criteriaType)
        {
            return _criteriaGroups.FirstOrDefault(x => x.Type == (byte)criteriaType)?.Criteria.FirstOrDefault();
        }

        protected IEnumerable<string> GetMultipleCriteria(ReportCriteriaType criteriaType)
        {
            return _criteriaGroups.Where(x => x.Type == (byte)criteriaType)?.SelectMany(x => x.Criteria)?.Select(criteria => criteria.Address);
        }

        protected void SetDateByValue(IEnumerable<string> addresses)
        {
            var success = false;
            foreach (var address in addresses)
            {
                var cellValue = _worksheet.Cells[address].Text;
                if (!DateTime.TryParseExact(cellValue, DateFormat,
                       ru,
                    DateTimeStyles.None, out var reportDate))
                {
                    continue;
                }

                Month = reportDate.Month;
                Year = reportDate.Year;
                ReportDate = reportDate;
                success = true;
            }
            if (!success)
            {
                throw new Exception("Невозможно определить дату отчёта");
            }
        }

        protected void SetDateByValue(string date)
        {
            if (!DateTime.TryParseExact(date, DateFormat,
                   ru,
                DateTimeStyles.None, out var reportDate))
            {
                throw new Exception("Невозможно определить дату отчёта");
            }

            Month = reportDate.Month;
            Year = reportDate.Year;
            ReportDate = reportDate;
        }

        public virtual void SetPeriods()
        {
        }


        private int? FindBeforeVirtRow()
        {
            for (var i = 1; i < _worksheet.Dimension.Rows; i++)
            {
                var value = _worksheet.Cells[i, 3].Value?.ToString().ToLower();
                if (!string.IsNullOrWhiteSpace(value) && value.Contains(UpdBeforeVirtText, StringComparison.InvariantCultureIgnoreCase))
                {
                    return i;
                }
            }
            return null;
        }
    }
}
