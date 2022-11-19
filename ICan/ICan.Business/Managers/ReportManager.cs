using AutoMapper;
using ExcelDataReader;
using ICan.Business.Services;
using ICan.Common;
using ICan.Common.Domain;
using ICan.Common.Models;
using ICan.Common.Models.Enums;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt;
using ICan.Common.Models.Opt.Report;
using ICan.Common.Repositories;
using ICan.Common.Utils;
using ICan.Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICan.Business.Managers
{
    public class ReportManager : BaseManager
    {
        private readonly ProductManager _productManager;
        private readonly WarehouseJournalManager _whJournalManager;
        private readonly IEmailSender _emailSender;
        private readonly ReportParseService _parseService;
        private readonly IWbReportRepository _wbReportRepository;
        private const int _startLeonardoColumn = 1;

        public ReportManager(IMapper mapper, ApplicationDbContext context, ILogger<BaseManager> logger,
            ProductManager productManager,
            WarehouseJournalManager whJournalManager,
            IWbReportRepository wbReportRepository,
            IConfiguration configuration,
            IEmailSender emailSender, ReportParseService parseService
            ) : base(mapper, context, logger, configuration)
        {
            _productManager = productManager;
            _whJournalManager = whJournalManager;
            _emailSender = emailSender;
            _parseService = parseService;
            _wbReportRepository = wbReportRepository;
        }

        public async Task<List<ReportModel>> GetReports()
        {
            var startDate = DateTime.Now.AddYears(-1);
            var reports = await _context.OptReport
                    .Include(report => report.Shop)
                    .Include(report => report.ReportKind)
                    .Where(report => report.ReportDate >= startDate)
                    .ToListAsync();
            return _mapper.Map<List<ReportModel>>(reports);
        }

        public async Task<byte[]> GetWbApiReportAsync(DateTime startDate, DateTime endDate)
        {
            var orders = _wbReportRepository.GetItems<OptWbOrder>(startDate, endDate);
            var sales = _wbReportRepository.GetItems<OptWbSale>(startDate, endDate);
            var products = await _productManager.GetAsync(false, dontShowDisabled: false);
            using (ExcelPackage objExcelPackage = new ExcelPackage())
            {
                foreach (var whName in WbManager.WhNames)
                {
                    var currentOrders = orders.Where(order => !string.IsNullOrWhiteSpace(order.WarehouseName) && whName.Contains(order.WarehouseName));
                    ExcelUtil.AddWbSheet(products, currentOrders, objExcelPackage.Workbook, $"{string.Join(",", whName)} Заказы", startDate, endDate);

                    var currentSales = sales.Where(sale => !string.IsNullOrWhiteSpace(sale.WarehouseName) && whName.Contains(sale.WarehouseName));
                    ExcelUtil.AddWbSheet(products, currentSales, objExcelPackage.Workbook, $"{string.Join(",", whName)} Продажи", startDate, endDate);
                }

                var bytes = objExcelPackage.GetAsByteArray();
                return bytes;
            }
        }

        public async Task<List<ReportModel>> GetUpdReports()
        {
            var currentYear = DateTime.Now.Year;
            var reports = await _context.OptReport
                    .Include(report => report.Shop)
                    .Include(report => report.ReportKind)
                    .Where(report => report.ReportKindId == (int)ReportKind.UPD && report.ReportDate.HasValue && report.ReportDate.Value.Year == currentYear)
                    .ToListAsync();
            return _mapper.Map<List<ReportModel>>(reports);
        }

        public async Task<ReportModel> GetReportDetails(string id)
        {
            var raw = await _context.OptReport
                 .Include(t => t.Shop)
                 .Include(t => t.ReportKind)
                 .Include(t => t.ReportItems)
                 .FirstAsync(report => report.ReportId == id);
            var model = _mapper.Map<ReportModel>(raw);
            var products = await _productManager.GetAsync(false, dontShowDisabled: false, onlyNotebooks: false);
            raw.ReportItems.ToList().ForEach(item =>
            {
                foreach (var group in products)
                {
                    var product = group.Value.FirstOrDefault(t => t.ProductId == item.ProductId);
                    if (product != null)
                    {
                        product.Amount = item.Amount;
                        product.TotalSum = item.TotalSum;
                    }
                }
            });
            model.Items = products;
            return model;
        }

        public async Task<List<ReportModel>> GetReportsPartial()
        {
            var reports = await _context.OptReport
                .Include(t => t.Shop)
                .Include(t => t.ReportKind)
                .ToListAsync();

            return _mapper.Map<List<ReportModel>>(reports);
        }

        [Obsolete]
        public async Task<byte[]> DownloadReport(int fromMonth, int fromYear, int toMonth, int toYear)
        {
            var startDate = new DateTime(fromYear, fromMonth, 1);
            var endDate = new DateTime(toYear, toMonth, DateTime.DaysInMonth(toYear, toMonth));

            var reports = _context.OptReport.Include(t => t.Shop)
                .Where(t => t.ShopId == (int)ShopType.Ozon && new DateTime(t.ReportYear, t.ReportMonth, 1) <= endDate &&
                  new DateTime(t.ReportYear, t.ReportMonth, 1) >= startDate)
               .OrderBy(t => t.ShopId);

            var products = await _productManager.GetAsync(false, dontShowDisabled: false);

            var helper = new ReportHelper
            {
                FromMonth = fromMonth,
                FromYear = fromYear,
                ToMonth = toMonth,
                ToYear = toYear,
                Products = products,
                ReportKinds = new[] { (int)ReportKind.Report, (int)ReportKind.UPD }
            };
            var warehouseItems = Enumerable.Empty<WarehouseItemModel>();

            IEnumerable<ReportItemModel> reportItems = await GetInternalItems(helper); // отчёты по покупкам  СП
            if (reports.Any())
            {
                var reportIds = reports.Select(q => q.ReportId).ToArray();
                var reportItemsRaw = _context.OptReportitem.Include(t => t.Report)
                    .Where(t => reportIds.Contains(t.ReportId));
                reportItems = reportItems.Union(_mapper.Map<IEnumerable<ReportItemModel>>(reportItemsRaw));
                var shopIds = reports.Select(tt => tt.ShopId).Distinct().ToList();
                warehouseItems = GetLatestWarehouseState(shopIds);
            }

            return GetReport(reports, reportItems, helper, warehouseItems);
        }

        private async Task<IEnumerable<ReportItemModel>> GetInternalItems(ReportHelper helper)
        {
            List<ReportItemModel> list = new List<ReportItemModel>();

            var orders = _context.OptOrder.Include(x => x.Client).Where(d =>
                 d.DoneDate.HasValue && d.DoneDate.Value.Year == helper.FromYear)
                 .Select(t => t.OrderId);

            await _context.OptOrderproduct
                .Include(q => q.Order)
                .ThenInclude(x => x.Client)
                  .Where(item => orders.Contains(item.OrderId))
                  .ForEachAsync(item => list.Add(new ReportItemModel
                  {
                      ProductId = item.ProductId,
                      Amount = item.Amount,
                      ReportMonth = item.Order.DoneDate.Value.Month,
                      ReportYear = item.Order.DoneDate.Value.Year,
                      ClientType = item.Order.Client.ClientType,
                      ReportTotalSum = item.Order.DiscountedSum,
                      OrderId = item.OrderId
                  }));

            return list;
        }

        public async Task RemoveOldUPD(string reportNum, int year)
        {
            var oldReports = _context.OptReport.Where(t =>
                               t.ReportKindId == (int)ReportKind.UPD
                               && t.ReportYear == year
                               && t.ReportNum == reportNum);

            if (oldReports != null)
            {
                await _whJournalManager.RemoveByAction(oldReports);
                _context.RemoveRange(oldReports);
            }
            await _context.SaveChangesAsync();
        }

        public async Task RemoveOldReport(Report uploadedData, bool useFormDates, int year)
        {
            var oldReports = _context.OptReport
                           .Where(t =>
                               t.ReportKindId == uploadedData.ReportKindId
                               && t.ReportYear == (useFormDates ? year : uploadedData.Year)
                               && t.ReportNum == uploadedData.ReportNum
                               && (t.ReportKindId == (int)ReportKind.UPD || t.ShopId == uploadedData.ShopId)
                               && ((t.ReportPeriodFrom.HasValue && uploadedData.ReportPeriodFrom.HasValue) ? t.ReportPeriodFrom.Value == uploadedData.ReportPeriodFrom : true)
                               && ((t.ReportPeriodTo.HasValue && uploadedData.ReportPeriodTo.HasValue) ? t.ReportPeriodTo.Value == uploadedData.ReportPeriodTo : true));
            if (oldReports != null && oldReports.Any())
            {
                await _whJournalManager.RemoveByAction(oldReports);
                _context.RemoveRange(oldReports);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<byte[]> GetConsolidatedWBReport(DateTime reportPeriodFrom)
        {
            var reports = await _context.OptReport
                .Where(x => x.ShopId == (int)ShopType.WB
                    && x.ReportPeriodFrom.HasValue
                    && x.ReportPeriodTo.HasValue
                    && x.ReportPeriodFrom.Value >= reportPeriodFrom)
                .Include(x => x.ReportItems).ToListAsync();
            var products = await _productManager.GetProductsByType("all");
            var consolidatedReport = new ConsolidatedWBReport(reports, products, reportPeriodFrom);
            var result = consolidatedReport.GenerateReport();
            return result;
        }

        public async Task SaveNewReport(Report uploadedData, bool useFormDates, int month, int year, string fileName)
        {
            var report = new OptReport
            {
                ReportMonth = useFormDates ? month : uploadedData.Month,
                ReportYear = useFormDates ? year : uploadedData.Year,
                UploadDate = DateTime.Now,
                ShopId = uploadedData.ShopId,
                ReportKindId = uploadedData.ReportKindId,
                TotalSum = uploadedData.TotalSum,
                PaidSum = uploadedData.PaidSum,
                ReportNum = uploadedData.ReportNum,
                FileName = fileName,
                IsVirtual = uploadedData.IsVirtual,
                ReportDate = uploadedData.ReportDate != null ? uploadedData.ReportDate : new DateTime(year, month, DateTime.DaysInMonth(year, month)),
                ReportId = Guid.NewGuid().ToString(),
                ReportPeriodFrom = uploadedData.ReportPeriodFrom,
                ReportPeriodTo = uploadedData.ReportPeriodTo
            };

            foreach (var item in uploadedData.ReportItems)
            {
                var reportItem = new OptReportitem
                {
                    ReportId = report.ReportId,
                    ProductId = item.NoteBookId,
                    Amount = item.Amount,
                    TotalSum = Math.Round(item.TotalSum, 2)
                };
                _context.Add(reportItem);
            }
            _context.Add(report);
            var needWriteToWhJournal = NeedWriteToWhJournal(report);

            if (needWriteToWhJournal)
            {
                var journal = uploadedData.ReportItems.Select(reportItem => new WarehouseJournalModel
                {
                    ActionDate = report.ReportDate.Value,
                    ActionTypeId = WhJournalActionType.Outcome,
                    ActionExtendedTypeId = WhJournalActionExtendedType.UPD,
                    ActionId = report.ReportId.ToString(),
                    ObjectTypeId = WhJournalObjectType.Notebook,
                    ObjectId = reportItem.NoteBookId,
                    Amount = reportItem.Amount,
                    WarehouseTypeId = WarehouseType.NotebookReady,
                    Comment = $"название файла: {report.FileName}, номер упд: {report.ReportNum}"
                });
                await _whJournalManager.AddRangeAsync(journal);
            }
            await _context.SaveChangesAsync();
        }

        public async Task UploadReport(IFormCollection collection)
        {
            var reportMonth = collection["Month"].ToString();
            var reportYear = collection["Year"].ToString();
            int year = 1700;
            int month = 1;

            if (!string.IsNullOrWhiteSpace(reportMonth) && !string.IsNullOrWhiteSpace(reportYear))
            {
                int.TryParse(reportMonth, out month);
                int.TryParse(reportYear, out year);
            }
            var warnings = new List<ReportWarning>();
            var shopNames = _context.OptShop.ToList();
            foreach (var file in collection.Files)
            {
                var uploadedData = await ParseFileAsync(file);
                var useFormDates = Report.ShopWithoutDates.Contains(uploadedData.ShopId)
                    && (uploadedData.ReportKindId != (int)ReportKind.UPD);

                if (uploadedData == null)
                    throw new UserException("Не найдено файлов для загрузки");
                if (uploadedData.UnknownShop)
                    throw new UserException("Не определён магазин");
             
                await RemoveOldReport(uploadedData, useFormDates, year);
                await SaveNewReport(uploadedData, useFormDates, month, year, file.FileName);
                var reportWarning = uploadedData
                    .Warning;

                warnings.Add(new ReportWarning { Fields = reportWarning.Fields, ShopId = reportWarning.ShopId, ShopName = shopNames.First(shop => shop.ShopId == uploadedData.ShopId).Name });
            }
            await _context.SaveChangesAsync();
            NotifyIfReportWasChanged(warnings);
        }

        public async Task<Report> ParseFileAsync(IFormFile file)
        {
            Report report = null;

            if (file.FileName.EndsWith("xls"))
            {
                report = await ReadFile(file);
            }
            else
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream).ConfigureAwait(false);

                    using (var package = new ExcelPackage(memoryStream))
                    {
                        var worksheet = package.Workbook.Worksheets[0]; // Tip: To access the first worksheet, try index 1, not 0
                        report = await ReadExcelPackage(worksheet);
                    }
                }
            return report;
        }

        public async Task RemoveReportById(string id)
        {
            var report = await _context.OptReport.FirstOrDefaultAsync(m => m.ReportId == id);
            if (report == null)
                return;
            var isUpd = report.ReportKindId == (int)ReportKind.UPD;
            _context.OptReport.Remove(report);

            if (isUpd)
            {
                await _whJournalManager.RemoveByAction(report.ReportId.ToString(), (int)WhJournalActionExtendedType.UPD);
            }
        }

        public async Task<Report> ParseFileAsync(byte[] data)
        {
            Report report = null;

            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(data, 0, data.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);

                using (var package = new ExcelPackage(memoryStream))
                {
                    var worksheet = package.Workbook.Worksheets[0]; // Tip: To access the first worksheet, try index 1, not 0
                    report = await ReadExcelPackage(worksheet);
                }

                return report;
            }
        }

        public async Task<IEnumerable<WbAggregateReport>> ReadWbAggregateFile(IFormFile file)
        {
            IEnumerable<WbAggregateReport> reports;
            if (file.FileName.ToLower().EndsWith("xls"))
            {
                reports = await ParseXLSWb(file);
            }
            else
            {
                reports = await ParseXLSXWb(file);
            }
            return reports;
        }

        public void NotifyIfReportWasChanged(List<ReportWarning> warnings)
        {
            if (warnings.Count == 0)
                return;
            var emails = _configuration["Settings:1C:Emails"].Split(",").ToList();
            var messageBody = new StringBuilder();
            foreach (var warning in warnings.Where(x => x.Fields.Count > 0))
            {
                messageBody.Append($"Магазин: <b>{warning.ShopName}</b> (ID: <b>{warning.ShopId}</b>) <br />" +
                    $"Поля: <br />");
                foreach (var field in warning.Fields)
                {
                    messageBody.Append($"<b>{field.FieldName}</b> Старое значение: <b>{field.OldAddress}</b> Новое значение: <b>{field.NewAddress}</b> <br />");
                }
                messageBody.Append("<br /><br />");
            }
            emails.ForEach(email => _emailSender.SendEmail(email, "Изменение полей в отчёте", messageBody.ToString()));
        }

        public TableDataResult<ReportModel> GetUpdReports(TableOptions options)
        {
            var pageSize = int.TryParse(options?.Limit, out var pageS) ? pageS : Const.PageSize;
            var offset = int.TryParse(options?.Offset, out var offS) ? offS : 0;
            var reports = _context.OptReport
                        .AsNoTracking()
                        .Include(report => report.Shop)
                        .Include(report => report.ReportKind)
                    .Where(report => report.ReportKindId == (int)ReportKind.UPD && !report.IsArhived)
                    .AsEnumerable();
            var filter = options.Filter;
            if (!string.IsNullOrWhiteSpace(filter))
            {
                reports = FilterReports(reports, filter);
            }
            if (!string.IsNullOrWhiteSpace(options.Sort))
            {
                reports = SortReports(reports, options);
            }

            var total = reports.Count();
            var result = reports.Skip(offset).Take(pageSize).ToList();

            var list = _mapper.Map<List<ReportModel>>(result);
            list = list.Select(upd =>
            {
                upd.ShopName = upd.ShopName.Replace("\"", "");
                return upd;
            }).ToList();
            return new TableDataResult<ReportModel> { Total = total, Rows = list };
        }

        private static IEnumerable<OptReport> SortReports(IEnumerable<OptReport> reports, TableOptions options)
        {
            var sort = options.Sort.Trim();
            var order = options.Order.Trim().ToLower() == "asc" ? SortOrder.Ascending : SortOrder.Descending;
            Func<OptReport, Type> predicate = null;
            if (sort.Equals("shopName"))
            {
                predicate = (OptReport report) => report.Shop.Name.GetType();
            }
            else if (sort.Equals("fileName"))
            {
                predicate = (OptReport report) => report.FileName.GetType();
            }
            else if (sort.Equals("reportNum"))
            {
                predicate = (OptReport report) => report.ReportNum.GetType();
            }
            else if (sort.Equals("reportDate"))
            {
                predicate = (OptReport report) => report.ReportDate.GetType();
            }
            else if (sort.Equals("reportYear"))
            {
                predicate = (OptReport report) => report.ReportYear.GetType();
            }
            else if (sort.Equals("isVirtual"))
            {
                predicate = (OptReport report) => report.IsVirtual.GetType();
            }
            else if (sort.Equals("totalSumFormatted"))
            {
                predicate = (OptReport report) => report.TotalSum.GetType();
            }
            else if (sort.Equals("uploadDate"))
            {
                predicate = (OptReport report) => report.UploadDate.GetType();
            }
            if (predicate == null)
                return reports;

            return order == SortOrder.Ascending ? reports.OrderBy(predicate) : reports.OrderByDescending(predicate);
        }

        private IEnumerable<OptReport> FilterReports(IEnumerable<OptReport> reports, string filterAsString)
        {
            var filter = JsonConvert.DeserializeObject<ReportFilter>(filterAsString);
            if (!string.IsNullOrWhiteSpace(filter.FileName))
                reports = reports.Where(report => !string.IsNullOrWhiteSpace(report.FileName) && report.FileName.Contains(filter.FileName));
            if (!string.IsNullOrWhiteSpace(filter.ReportNum))
                reports = reports.Where(x => x.ReportNum.Equals(filter.ReportNum));
            if (!string.IsNullOrWhiteSpace(filter.ShopName))
            {
                reports = reports.Where(upd => upd.Shop.Name.Replace("\"", "").Equals(filter.ShopName));
            }
            if (filter.ReportYear != 0)
                reports = reports.Where(x => x.ReportYear == filter.ReportYear);
            if (!string.IsNullOrWhiteSpace(filter.ReportDate))
            {
                reports = reports.Where(x => x.ReportDate.ToString().Contains(filter.ReportDate));
            }
            if (!string.IsNullOrWhiteSpace(filter.UploadDate))
            {
                reports = reports.Where(x => x.UploadDate.ToString("dd:MM:yyyy hh:mm").Contains(filter.UploadDate));
            }

            return reports;
        }


        public byte[] GetReport(IOrderedQueryable<OptReport> reports,
            IEnumerable<ReportItemModel> reportItems,
            ReportHelper helper,
            IEnumerable<WarehouseItemModel> warehouseItems)
        {
            var startDate = new DateTime(helper.FromYear, helper.FromMonth, 1);
            var endDate = new DateTime(helper.ToYear, helper.ToMonth, DateTime.DaysInMonth(helper.ToYear, helper.ToMonth));
            byte[] bytes = new byte[0];
            //var shops = _context.OptShop.ToList();
            //using (ExcelPackage objExcelPackage = new ExcelPackage())
            //{
            //	ExcelWorksheet objWorksheet;
            //	var innerWarehouseItemsRaw = _context.OptWarehouseItem.Include(t => t.Warehouse)
            //		.Where(t => t.Warehouse.ShopId == null && t.Warehouse.DateAdd >= startDate && t.Warehouse.DateAdd <= endDate);
            //	var innerWarehouseItems = _mapper.Map<IEnumerable<WarehouseItemModel>>(innerWarehouseItemsRaw);
            //	var orderPayments = _context.OptOrderpayment;
            //	ExcelUtil.AddWorkSheet("Общий", objExcelPackage.Workbook, reportItems, helper, innerWarehouseItems, true);
            //	ExcelUtil.AddWorkSheet("Модуль", objExcelPackage.Workbook, reportItems.Where(t => string.IsNullOrWhiteSpace(t.ReportId)), helper, showRemainigs: false, showPaid: true, orderPayments: orderPayments);

            //	if (reports.Any())
            //	{
            //		var commonReportItems = reportItems.Where(t => t.ReportKindId.HasValue && t.ReportKindId.Value != (int)ReportKind.UPD);
            //		var updReportItems = reportItems.Where(t => t.ReportKindId.HasValue && t.ReportKindId.Value == (int)ReportKind.UPD);

            //		foreach (var shopId in new[] { ShopType.Ozon })
            //		{
            //			var shop = shops.First(t => t.ShopId == (int)shopId);
            //			objWorksheet = objExcelPackage.Workbook.Worksheets.Add(shop.Name);

            //			ExcelUtil.PrintHeader(objWorksheet, helper);
            //			ExcelUtil.PrintContents(objWorksheet, helper,
            //				commonReportItems.Where(t => t.ShopId == (int)shopId),
            //				updReportItems.Where(t => t.ShopId == (int)shopId),
            //				warehouseItems?.Where(t => t.ShopId == (int)shopId));

            //			var printPaidSum = Report.ShopsWithPaidSum.Contains(shopId);

            //			ExcelUtil.PrintFooter(objWorksheet, helper, reports.Where(t => t.ShopId == (int)shopId
            //				&& t.ReportKindId == (int)ReportKind.Report), printPaidSum);
            //			objWorksheet.Cells.AutoFitColumns(5.0, 40.0);
            //		}

            //	}
            //	bytes = objExcelPackage.GetAsByteArray();
            //}
            return bytes;
        }

        public async Task<List<LeonardoAggReport>> ParseLeonardoFileAsync(IFormFile file)
        {
            List<LeonardoAggReport> reports = new List<LeonardoAggReport>();
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream).ConfigureAwait(false);

                using (var package = new ExcelPackage(memoryStream))
                {
                    foreach (var worksheet in package.Workbook.Worksheets)
                    {
                        reports.Add(await ReadLeonardoExcelPackage(worksheet));
                    }
                }
            }
            return reports;
        }

        public async Task<byte[]> GetWbTransformedFile(IEnumerable<WbAggregateReport> uploadedData)
        {
            byte[] bytes = new byte[0];

            using (ExcelPackage objExcelPackage = new ExcelPackage())
            {
                ExcelWorksheet objWorksheet = objExcelPackage.Workbook.Worksheets.Add("Общий");

                var productGroups = await _productManager.GetAsync(false, dontShowDisabled: false, onlyNotebooks: true);

                ExcelUtil.PrintWbProducts(productGroups, objWorksheet, uploadedData);

                bytes = objExcelPackage.GetAsByteArray();
            }
            return bytes;
        }

        public byte[] GetLeonardoTotalReport(IEnumerable<LeonardoAggReport> reports)
        {
            byte[] bytes = new byte[0];
            var products = _context.OptProduct
                  .Include(t => t.ProductSeries).Where(t => t.ProductKindId == 1 /*Тетрадь*/
             && !string.IsNullOrWhiteSpace(t.ISBN) && t.ISBN.Length > 3)
             .Select(product => new ReportProductModel
             {
                 ProductId = product.ProductId,
                 Name = product.Name,
                 SeriesName = product.ProductSeries.Name,
                 SeriesOrder = product.ProductSeries.Order,
                 ISBN = product.ISBN
             }).OrderBy(t => t.SeriesOrder).ThenBy(t => t.ProductId).ToList();


            using (ExcelPackage objExcelPackage = new ExcelPackage())
            {
                ExcelWorksheet objWorksheet = objExcelPackage.Workbook.Worksheets.Add("Общий");
                ExcelUtil.PrintLeonardoHeader(products, _startLeonardoColumn, objWorksheet, reports.ToArray());
                bytes = objExcelPackage.GetAsByteArray();
            }
            return bytes;
        }

        public IEnumerable<WarehouseItemModel> GetLatestWarehouseState(IEnumerable<int> shopIds)
        {
            List<WarehouseItemModel> warehouseItems = new List<WarehouseItemModel>();
            //var warehousesIds = new List<int>();

            //foreach (var shopId in shopIds)
            //{
            //	var latestState = _context.OptWarehouse.Where(t => t.ShopId == shopId).OrderByDescending(whouse => whouse.DateAdd).FirstOrDefault();
            //	if (latestState != null)
            //	{
            //		warehousesIds.Add(latestState.WarehouseId);
            //	}
            //}
            //warehouseItems = _mapper.Map<List<WarehouseItemModel>>(_context.OptWarehouseItem.Include(t => t.Warehouse)
            //		.Where(t => warehousesIds.Contains(t.WarehouseId)));
            return warehouseItems;
        }

        public async Task<byte[]> GetShipmentReport(ReportHelper helper)
        {
            Dictionary<int, int> shopIdWithReportKind = new Dictionary<int, int>()
            {
                {(int)ShopType.Ozon, (int)ReportKind.Report},
                {(int)ShopType.WB, (int)ReportKind.Report},
                {(int)ShopType.ReadingCity, (int)ReportKind.Report},
                {(int)ShopType.MyShop, (int)ReportKind.Report},
                {(int)ShopType.Eksmo,  (int)ReportKind.UPD},
                {(int)ShopType.Leonardo,  (int)ReportKind.UPD},
                {(int)ShopType.Waitex,  (int)ReportKind.UPD},
                {(int)ShopType.VkusVill,  (int)ReportKind.UPD },
            };
            helper.ShopWithReportKind = shopIdWithReportKind;
            var allShops = _context.OptShop.Where(shop => helper.ShopWithReportKind.Keys.Contains(shop.ShopId)).ToList();
            var shopsWithReport = allShops.Where(
                                            x => helper.ShopWithReportKind.Where(w => w.Value == (int)ReportKind.Report).Select(k => k.Key).ToList().Contains(x.ShopId))
                                            .ToList();
            var shopsWithUpd = allShops.Where(
                                            x => helper.ShopWithReportKind.Where(w => w.Value == (int)ReportKind.UPD).Select(k => k.Key).ToList().Contains(x.ShopId))
                                            .ToList();

            var shopIdWithName = shopsWithReport.Union(shopsWithUpd).ToDictionary(k => k.ShopId, v => v.Name);
            // what if no such reports            
            var reports = _context.OptReport.Where(t => shopIdWithName.Keys.Contains(t.ShopId)
                && t.ReportYear == helper.FromYear);

            List<string> reportIds = new List<string>();
            foreach (var KVPair in helper.ShopWithReportKind)
            {
                foreach (var report in reports)
                {
                    if (report.ShopId == KVPair.Key && report.ReportKindId == KVPair.Value)
                        reportIds.Add(report.ReportId);
                }
            }

            var reportItems = _mapper.Map<IEnumerable<ReportItemModel>>(_context.OptReportitem
                .Include(t => t.Report)
                .Where(t => reportIds.Contains(t.ReportId)));

            var internalReportItems = await GetInternalItems(helper);
            reportItems = reportItems.Union(internalReportItems);

            return ExcelUtil.PrintContents(helper, shopIdWithName, reportItems);
        }

        public async Task<byte[]> GetPrimerReport(int year, BobReportTypeEnum type)
        {
            var reportHelper = await GetPrimerHelper(year);
            var primerReport = type == BobReportTypeEnum.QuantityAndSum ? new PrimerReport(reportHelper) :
                    new PrimerReportQty(reportHelper);
            var result = primerReport.GetReport();
            return result;
        }

        private async Task<IEnumerable<WbAggregateReport>> ParseXLSXWb(IFormFile file)
        {
            var reports = new List<WbAggregateReport>();
            var productsWithIds = await _productManager.GetProductsWithIsbn();
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream).ConfigureAwait(false);

                using (var package = new ExcelPackage(memoryStream))
                {
                    var worksheet = package.Workbook.Worksheets[0];

                    foreach (WbCity city in Enum.GetValues(typeof(WbCity)).Cast<WbCity>()
                        .Where(ci => ci != WbCity.None))
                    {
                        var report = new WbAggregateXLSXReport(
                              worksheet, null, (int)ShopType.WB, ReportKind.Report, city, productsWithIds: productsWithIds);
                        report.SetShopItems();

                        reports.Add(report);
                    }
                }
            }
            return reports;
        }

        private async Task<IEnumerable<WbAggregateReport>> ParseXLSWb(IFormFile file)
        {
            var reports = new List<WbAggregateReport>();
            var productsWithIds = await _productManager.GetProductsWithIsbn();
            string filePath = await PrepareXlSXFile(file);
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    DataSet result = reader.AsDataSet();
                    foreach (WbCity city in Enum.GetValues(typeof(WbCity)).Cast<WbCity>()
                        .Where(ci => ci != WbCity.None))
                    {
                        var report = new WbAggregateXLSReport(
                          null, result, (int)ShopType.WB, ReportKind.Report, city, productsWithIds: productsWithIds);
                        report.SetShopItems();

                        reports.Add(report);
                    }
                }
            }
            return reports;
        }

        private async Task<PrimerReportHelper> GetPrimerHelper(int year)
        {
            var reportHelper = new PrimerReportHelper { Year = year, ProductId = 47 };

            var orders = _context.OptOrder
                    .Where(x => x.DoneDate.Value.Year == year)
                    .Include(x => x.OptOrderproducts)
                        .ThenInclude(x => x.ProductPrice)
                    .Include(x => x.Client)
                        .ThenInclude(x => x.ApplicationUserShopRelations)
                        .ThenInclude(x => x.Shop)
                    .Where(x => (x.Client.ClientType == (int)ClientType.JointPurchase || x.Client.ClientType == (int)ClientType.Shop)
                        && x.OptOrderproducts.Any(x => x.ProductId == reportHelper.ProductId));

            reportHelper.SPOrders = await orders
                    .Where(x => x.Client.ClientType == (int)ClientType.JointPurchase)
                    .ToListAsync();

            reportHelper.ShopOrders = await orders
                    .Where(x => x.Client.ClientType == (int)ClientType.Shop && x.Client.ApplicationUserShopRelations.Count() == 0)
                    .ToListAsync();

            var reports = _context.OptReportitem
                    .Include(x => x.Report)
                        .ThenInclude(x => x.ReportKind)
                    .Where(x => x.ProductId == reportHelper.ProductId
                    && (x.Report.ShopId == (int)ShopType.Ozon || x.Report.ShopId == (int)ShopType.WB)
                    && x.Report.ReportYear == year
                    && x.Report.ReportKind.ReportKindId == (int)ReportKind.UPD);

            reportHelper.OzonReportItems = await reports
                    .Where(x => x.Report.ShopId == (int)ShopType.Ozon)
                    .ToListAsync();

            reportHelper.WBReportItems = await reports
                    .Where(x => x.Report.ShopId == (int)ShopType.WB)
                        .ToListAsync();

            reportHelper.UPDReportItems = await _context.OptReportitem
                    .Include(x => x.Report)
                        .ThenInclude(x => x.ReportKind)
                    .Where(x => (x.Report.ShopId != (int)ShopType.Ozon && x.Report.ShopId != (int)ShopType.WB)
                        && x.Report.ReportKind.ReportKindId == (int)ReportKind.UPD
                        && x.Report.ReportYear == year)
                    .ToListAsync();

            reportHelper.whJournalItems = await _context.OptWarehouse.Include(x => x.WarehouseItems)
                    .Where(x => x.DateAdd.Year == year && (x.WarehouseActionTypeId == (int)WarehouseActionType.Marketing ||
                    x.WarehouseActionTypeId == (int)WarehouseActionType.Returning) && x.WarehouseItems.Any(x => x.ProductId == reportHelper.ProductId))
                    .ToListAsync();

            return reportHelper;
        }

        private async Task<LeonardoAggReport> ReadLeonardoExcelPackage(ExcelWorksheet worksheet)
        {
            var rowCount = worksheet.Dimension?.Rows;
            var colCount = worksheet.Dimension?.Columns;

            if (!rowCount.HasValue || !colCount.HasValue)
            {
                return null;
            }
            var productWithIds = await _productManager.GetProductsWithIsbn();
            var report = new LeonardoAggReport(worksheet, null, (int)ShopType.Leonardo, ReportKind.Report, productsWithIds: productWithIds)
            {
                TabName = worksheet.Name
            };
            report.SetShopItems();
            report.SetLocalShopName();

            return report;
        }

        private async Task<Report> ReadFile(IFormFile file)
        {
            string filePath = await PrepareXlSXFile(file);

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    DataSet result = reader.AsDataSet();
                    var report = await _parseService.GetShopReport(result);

                    FillData(report);
                    return report;
                }
            }
        }

        private async Task<string> PrepareXlSXFile(IFormFile file)
        {
            var tempXLSDir = _configuration["Settings:TempFileDirectory"];
            Directory.EnumerateFiles(tempXLSDir).ToList()
                .ForEach(tempfile => File.Delete(tempfile));

            var filePath = Path.Combine(tempXLSDir, file.FileName);
            var newFilePath = filePath + "x";
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return filePath;
        }

        private async Task<Report> ReadExcelPackage(ExcelWorksheet worksheet)
        {
            var rowCount = worksheet.Dimension?.Rows;
            var colCount = worksheet.Dimension?.Columns;

            if (!rowCount.HasValue || !colCount.HasValue)
            {
                return Report.Empty();
            }

            var report = await _parseService.GetShopReport(worksheet);
            if (report.IgnoreUpd) { 
                return report;
            }

            if (report.NeedUpload)
            {
                FillData(report);
            }
            else if (report.UnknownShop)
            {
                report.SetShopItems();
            }
            return report;
        }

        private void FillData(Report report)
        {
            report.SetReportDate();
            report.SetReportNum();
            report.SetShopItems();
            report.SetVirtAttr();
            report.SetTotalSum();
            report.SetPaidSum();
            report.SetPeriods();
        }

        private bool NeedWriteToWhJournal(OptReport report)
        {
            if (report.ReportKindId != (int)ReportKind.UPD || (report.IsVirtual != null
                && report.IsVirtual.Value))
                return false;

            //var ignoreShopIdList = _context.OptShop.Where(shop => shop.IgnoreInWarehouseCalc)
            //	.Select(shop => shop.ShopId).ToList();

            var usedInOrder = _context.OptOrder
              .FirstOrDefault(order => !string.IsNullOrWhiteSpace(order.UpdNum)
              && order.UpdNum.Equals(report.ReportNum)
              && order.OrderDate.Year == report.ReportYear);

            //var customerHaveShop = _context.Users.Any(user => user.ShopID == report.ShopId);

            return /*!ignoreShopIdList.Contains(report.ShopId) &&*/ usedInOrder == null;
            //&& !customerHaveShop; 
        }
    }
}