using ICan.Business.Managers;
using ICan.Common;
using ICan.Common.Models.Exceptions;
using ICan.Common.Models.Opt.Report;
using ICan.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ICan.Business.Services
{
    public class ReportParseService
    {
        private static readonly string UpdCell = "B1";
        private static readonly List<string> UpdDatesCells
            = new List<string> { "Z1", "Y1" };
        private static readonly string UpdInnCell = "V12";
        private static readonly string UpdСonsigneeCell = "V8";
        private static readonly string UpdSecondСonsigneeCell = "R8";
        private static readonly List<string> UpdWords = new List<string> { "универсальный", "передаточный", "документ" };

        private static readonly List<(ShopType Shop, ReportKind ReportKind, ExcelType ExcelType, Type ReportType)> _reportTypes = new List<ValueTuple<ShopType, ReportKind, ExcelType, Type>>
        {
            (ShopType.WB, ReportKind.Report, ExcelType.XLSX, typeof(WbReport)),
            (ShopType.WB, ReportKind.ReportByPeriod, ExcelType.XLSX, typeof(WBReportByPeriod)),
            (ShopType.WB, ReportKind.Report, ExcelType.XLS, typeof(WbXLSReport)),
            (ShopType.WB, ReportKind.UPD,  ExcelType.XLSX,typeof(LongIdFormatUpdReport)),
            (ShopType.ReadingCity, ReportKind.Report, ExcelType.XLSX, typeof(ReadingCityReport)),
            (ShopType.ReadingCity, ReportKind.Report, ExcelType.XLS, typeof(ReadingCityXLSReport)),
            (ShopType.ReadingCity, ReportKind.UPD, ExcelType.XLSX, typeof(LongIdFormatUpdReport)),
            (ShopType.Ozon, ReportKind.Report,ExcelType.XLSX,  typeof(OzonReport)),
            (ShopType.MyShop, ReportKind.Report, ExcelType.XLSX,  typeof(MyShopReport)),
        };

        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportParseService> _logger;
        private readonly ProductManager _productManager;
        private readonly ReportCriteriaService _criteriaService;

        public ReportParseService(ApplicationDbContext context,
            ReportCriteriaService criteriaService,
            ProductManager productManager,
            ILogger<ReportParseService> logger)
        {
            _context = context;
            _logger = logger;
            _productManager = productManager;
            _criteriaService = criteriaService;
        }

        public async Task<Report> GetShopReport(ExcelWorksheet worksheet)
        {
            ParsedReport result = new ParsedReport();
            var excelType = ExcelType.XLSX;
            try
            {
                result = FindXLSXReport(worksheet);
                if (result.ReportKind == ReportKind.UPD && !result.ShopIsFound)
                {
                    var report = new UpdReport(worksheet, null, result.ShopId, result.ReportKind)
                    {
                        NeedUpload = false,
                        IgnoreUpd = result.IgnoreUpd
                    };
                    return report;
                }

                return await GetReportInternal(worksheet, null, result, excelType);
            }
            catch (Exception ex)
            {
                _logger.LogError(Const.ErrorMessages.CantRecognizeReportType, ex);
                throw;
            }
        }

        public async Task<Report> GetShopReport(DataSet data)
        {
            var error = new UnknownReportException(Const.ErrorMessages.CantRecognizeReportType);
            var found = false;
            int shopId = (int)ShopType.None;
            ReportKind reportKind = ReportKind.None;
            try
            {
                found = FindXLSReport(data, out shopId, out reportKind);
                var parsedReport = new ParsedReport {
                    ShopId = shopId,
                    ReportKind = reportKind,
                    ShopIsFound = found
                };
                return await GetReportInternal( null, data, parsedReport, ExcelType.XLS);
            }
            catch (Exception ex)
            {
                _logger.LogError(Const.ErrorMessages.CantRecognizeReportType, ex);
                throw;
            }
        }

        private async Task<Report> GetReportInternal(ExcelWorksheet worksheet, DataSet data,
            ParsedReport parsedData, ExcelType excelType)
        {
            var error = new UnknownReportException(Const.ErrorMessages.CantRecognizeReportType);
            if (!parsedData.ShopIsFound)
                throw error;

            var report = _reportTypes.FirstOrDefault(t => (int)t.Shop == parsedData.ShopId
                && t.ExcelType == excelType
                && t.ReportKind == parsedData.ReportKind);

            var reportType = parsedData.ReportKind == ReportKind.UPD ? GetUpdReportType(worksheet, report) : report.ReportType;

            Report instance = null;
            var criteriaGroups = _criteriaService.GetCriteriaGroupsForReport(parsedData.ShopId);
            var productWithIds = await _productManager.GetProductsWithIsbn();
            instance = (Report)Activator.CreateInstance(reportType,
                worksheet, data, parsedData.ShopId, parsedData.ReportKind, criteriaGroups, productWithIds);
            instance.IgnoreUpd = parsedData.IgnoreUpd;
            return instance;
        }

        private ParsedReport FindXLSXReport(
            ExcelWorksheet worksheet)
        {
            var result = new ParsedReport();
            var found = false;
            var criteria = _criteriaService.GetCriteria();

            try
            {
                var isUpd = CheckUpd(worksheet);
                if (isUpd)
                {
                    result.ReportKind = ReportKind.UPD;
                    var isFound = TryFindUpdShop(worksheet, out var shopId, out var ignoreUpd);
                    result.ShopIsFound = isFound;
                    result.ShopId = shopId;
                    result.IgnoreUpd = ignoreUpd;
                    return result;
                }

                foreach (var criteriaItem in criteria)
                {
                    foreach (var reportItems in criteriaItem.Value) /// shop => it's reports kinds
					{
                        _logger.LogWarning($"criteria {criteriaItem.Key}, reportItems {reportItems.Key}");
                        try
                        {
                            found = reportItems.Value.All(item =>
                            {
                                var cellValue = worksheet.Cells[item.Key]?.Value?.ToString();
                                return (!string.IsNullOrWhiteSpace(cellValue) &&
                                 cellValue.ToLower().Trim().Contains(item.Value.ToLower().Trim()));
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "ой");
                            found = false;
                        }
                        if (found)
                        {
                            result.ShopIsFound = found;
                            result.ShopId = (int)criteriaItem.Key;
                            result.ReportKind = reportItems.Key;
                            break;
                        }
                    }
                    if (found)
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(Const.ErrorMessages.CantRecognizeReportType, ex);
                throw new ArgumentException(Const.ErrorMessages.CantRecognizeReportType);
            }
            return result;
        }

        private bool FindXLSReport(DataSet data, out int shopId, out ReportKind reportKind)
        {
            var found = false;
            shopId = (int)ShopType.None;
            reportKind = ReportKind.None;

            var criteriaXLS = _criteriaService.GetXLSCriteria();

            try
            {
                foreach (var criteriaItem in criteriaXLS)
                {
                    foreach (var reportItems in criteriaItem.Value)
                    {
                        _logger.LogTrace($"criteriaItem {criteriaItem.Key}, reportItems {reportItems.Key}");
                        found =
                            reportItems.Value.All(item =>
                            {
                                var cellValue = data.Tables[0].Rows[item.Row][item.Col]?.ToString();
                                return (!string.IsNullOrWhiteSpace(cellValue) &&
                                     cellValue.ToLower().Trim().Contains(item.Text.ToLower().Trim()));
                            });
                        if (found)
                        {
                            shopId = (int)criteriaItem.Key;
                            reportKind = reportItems.Key;
                            break;
                        }
                    }
                    if (found)
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(Const.ErrorMessages.CantRecognizeReportType, ex);
                throw new ArgumentException(Const.ErrorMessages.CantRecognizeReportType);
            }
            return found;
        }


        private bool CheckUpd(ExcelWorksheet worksheet)
        {
            var updCellValue = worksheet.Cells[UpdCell]?
                .Value?.ToString().Trim().ToLower();
            if (string.IsNullOrWhiteSpace(updCellValue) ||
                 (!UpdWords.All(word => updCellValue.Contains(word))))
                return false;
            return true;
        }

        private Type GetUpdReportType(ExcelWorksheet worksheet, (ShopType Shop, ReportKind ReportKind, ExcelType ExcelType, Type ReportType) defaultReport)
        {
            //report.Equals(default) 
            foreach (var key in UpdDatesCells)
            {
                var cellValue = worksheet.Cells[key].Value;
                if (DateTime.TryParseExact(cellValue?.ToString(), "d MMMM yyyy г.", null, System.Globalization.DateTimeStyles.None, out var date))
                {
                    if (date > new DateTime(2021, 7, 1))
                        return typeof(AlternateUpdReport);
                }
            }
            return defaultReport.Equals(default) ? typeof(UpdReport) : defaultReport.ReportType;
        }


        private bool TryFindUpdShop(ExcelWorksheet worksheet, out int shopId, out bool ignoreUpd)
        {
            shopId = (int)ShopType.None;
            ignoreUpd = false;
            var innAndKppValue = worksheet.Cells[UpdInnCell]?.Value?.ToString();
            if (string.IsNullOrWhiteSpace(innAndKppValue))
            {
                innAndKppValue = worksheet.Cells["BE6"]?.Value?.ToString() ?? string.Empty;
            }
            if (!string.IsNullOrWhiteSpace(innAndKppValue))
            {
                var inn = innAndKppValue.Split("/")[0];
                if (string.IsNullOrWhiteSpace(inn))
                    return false;
                var shopName = _context.OptShopName
                    .Include(shopN => shopN.Shop)
                    .FirstOrDefault(shopNameItem =>
                     !string.IsNullOrWhiteSpace(shopNameItem.Inn) &&
                     string.Equals(shopNameItem.Inn.Trim(), inn.Trim()));
                if (shopName != null)
                {
                    ignoreUpd = shopName.Shop.IgnoreInWarehouseCalc;
                    shopId = shopName.ShopId;
                    return true;
                }
            }

            var consignee = worksheet.Cells[UpdСonsigneeCell]?.Value?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(consignee))
            {
                consignee = worksheet.Cells[UpdSecondСonsigneeCell]?.Value?.ToString() ?? string.Empty;
            }
            if (string.IsNullOrWhiteSpace(consignee))
            {
                return false;
            }
            consignee = consignee.Replace(" ", "").ToLowerInvariant().Trim();
            var availableShops =
                _context.OptShop.Include(sItem => sItem.ShopNames).ToList();

            var shop = availableShops.FirstOrDefault(sItem =>
                sItem.ShopNames.Any(shopNameItem => consignee.Contains(shopNameItem.Name.Replace(" ", "").ToLowerInvariant().Trim())));

            if (shop != null)
            {
                shopId = shop.ShopId;
                ignoreUpd = shop.IgnoreInWarehouseCalc;
                return true;
            }

            return false;
        }
    }
}
