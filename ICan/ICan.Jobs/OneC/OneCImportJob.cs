using ICan.Business.Managers;
using ICan.Business.Services;
using ICan.Common.Models.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ICan.Jobs.OneC
{
    public class OneCImportJob
    {
        private readonly ILogger _logger;
        private readonly ReportManager _reportManager;
        private readonly IEmailSender _emailSender;
        private readonly string FtpAddress;
        private readonly string Emails;
        private readonly bool ShouldRename;
        private readonly WebClient _webClient;
        private readonly ICredentials _networkCredentials;
        private OneCImportState _importState;

        public OneCImportJob(
            ILogger<OneCImportJob> logger,
            IConfiguration configuration,
            IEmailSender emailSender,
            ReportManager reportManager)
        {
            _logger = logger;
            _reportManager = reportManager;
            _emailSender = emailSender;
            FtpAddress = configuration["Settings:1C:FtpAddress"];
            var login = configuration["Settings:1C:FtpLogin"];
            var password = configuration["Settings:1C:FtpPassword"];
            _networkCredentials = new NetworkCredential(login, password);
            _webClient = new WebClient
            {
                Credentials = _networkCredentials
            };
            Emails = configuration["Settings:1C:Emails"];
            ShouldRename = bool.TryParse(configuration["Settings:1C:ShouldRename"], out var shouldRename) && shouldRename;
        }

        public bool Import()
        {
            var result = true;
            IEnumerable<LocalFileInfo> fileNames;
            try
            {
                fileNames = GetFileNamesFromFTP();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FromFTP] Ошибка при получении списка файлов с FTP");
                result = false;
                return result;
            }

            var xlsxFiles = fileNames.Where(item => item.FileName.EndsWith(".xlsx"));
            if (!fileNames.Any() || !xlsxFiles.Any())
            {
                _logger.LogInformation("[FromFTP] нет файлов на FTP");
                return result;
            }

            _importState = new OneCImportState
            {
                TotalAmount = xlsxFiles.Count()
            };

            ProcessFileNames(xlsxFiles);
            SendMessage();
            return result;
        }

        private void ProcessFileNames(IEnumerable<LocalFileInfo> xlsxFiles)
        {
            try
            {
                foreach (var fileInfo in xlsxFiles.OrderBy(file => file.Date))
                {
                    Thread.Sleep(3500);
                    try
                    {
                        var importResult = ProcessFtpFile(fileInfo.FileName).Result;
                        _importState.ProcessedFileNames.Add(fileInfo.FileName);
                        AddState(importResult, fileInfo.FileName);
                    }
                    catch (UserException)
                    {
                        _importState.UnknownShopFileNames.Add(fileInfo?.FileName);
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null && ex.InnerException is MySqlException mysqlEx)
                        {
                            _logger.LogError(mysqlEx, $"[FromFTP] Ошибка при cохранении информации из файла в БД {fileInfo?.FileName}");
                            _importState.ErrorFileNames.Add(fileInfo?.FileName);
                        }
                        else
                        {
                            _logger.LogWarning(ex, $"[FromFTP] Ошибка при обработке файла {fileInfo?.FileName}");
                        }
                        _importState.ErrorFileNames.Add($"{fileInfo?.FileName} {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FromFTP] Ошибка при cохранении информации из файла с FTP");
            }
        }


        private void AddState(OneCImportResult importResult, string fileName)
        {
            switch (importResult)
            {
                case OneCImportResult.Done:
                    _importState.DoneFileNames.Add(fileName);
                    break;
                case OneCImportResult.Removed:
                    _importState.RemovedFileNames.Add(fileName);
                    break;
                case OneCImportResult.UnknowShop:
                    _importState.UnknownShopFileNames.Add(fileName);
                    break;
                case OneCImportResult.Skipped:
                    _importState.SkippedFileNames.Add(fileName);
                    break;
                case OneCImportResult.VirtSkipped:
                    _importState.VirtSkippedFileNames.Add(fileName);
                    break;
            };
        }

        private IEnumerable<LocalFileInfo> GetFileNamesFromFTP()
        {
            IEnumerable<LocalFileInfo> fileNames = Enumerable.Empty<LocalFileInfo>();
            var request = (FtpWebRequest)WebRequest.Create(FtpAddress);
            request.Credentials = _networkCredentials;
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            var files = new List<LocalFileInfo>();

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                using Stream responseStream = response.GetResponseStream();
                using StreamReader reader = new StreamReader(responseStream);
                var names = reader.ReadToEnd();

                var rawList = names.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                rawList.ForEach(item => files.Add(LocalFileInfo.Build(item)));
            }
            return files;
        }

        private async Task<OneCImportResult> ProcessFtpFile(string fileName)
        {
            string url = Path.Combine(FtpAddress, fileName);
            byte[] contents = _webClient.DownloadData(url);
            if (fileName.StartsWith("delete"))
            {
                await RemoveUpd(fileName);
                return OneCImportResult.Removed;
            }

            var uploadedData = await _reportManager.ParseFileAsync(contents);

            if (uploadedData.UnknownShop)
            {
                return OneCImportResult.UnknowShop;
            }
            if (uploadedData.IgnoreUpd)
            {
                return OneCImportResult.Skipped;
            }
            else
            {
                await _reportManager.RemoveOldReport(uploadedData, false, -1);
                await _reportManager.SaveNewReport(uploadedData, false, -1, -1, fileName);
                if (uploadedData.IsVirtual)
                {
                    return OneCImportResult.VirtSkipped;
                }
                else
                {
                    return OneCImportResult.Done;
                }
            }
        }

        private async Task RemoveUpd(string fileName)
        {
            var regex = new Regex("0000-000[0-9]{1,3}");

            //by int.Parse we remove leadng zeros 

            var num = int.Parse(regex.Matches(fileName).First().Value.Replace("-", "")).ToString();
            var lastIndexofPeriod = fileName.LastIndexOf(".");
            var year = int.Parse(fileName.Substring(lastIndexofPeriod - 4, 4));
            await _reportManager.RemoveOldUPD(num, year);
        }

        private void SendMessage()
        {
            var log = new StringBuilder();

            log.Append($"Всего УПД: {_importState.TotalAmount} <br />" +
                        $"Всего обработано: {_importState.ProcessedAmount}. <br />"
                        + $"Из них импортировано: {_importState.DoneAmount} <br />"
                        + $"		удалено: {_importState.RemovedAmount} <br />");

            if (_importState.RemovedAmount > 0)
            {
                log.Append($"удалённые упд <br /> "
                        + $"{string.Join(",<br />", _importState.RemovedFileNames)} <br />  <br /> ");
            }
            
            log.Append($"		пропущено: {_importState.SkippedAmount} <br />");
            if (_importState.SkippedAmount > 0)
            {
                log.Append($"пропущенные файлы <br /> "
                        + $"{string.Join(",<br />", _importState.SkippedFileNames)} <br />  <br /> ");
            }

            log.Append($"виртуальных упд: {_importState.VirtSkippedAmount} <br />"
                        + $"		НЕ определён магазин для: {_importState.UnknownShopAmount} <br />");
            if (_importState.UnknownShopAmount > 0)
            {
                log.Append($" не определён магазин в следующих файлах <br /> "
                        + $"{string.Join(",<br />", _importState.UnknownShopFileNames)} <br />  <br /> ");
            }

            log.Append($"=============================================== <br />"
                        + $"Возникла ошибка для: {_importState.ErrorAmount}. <br />");

            log.Append("Длинные списки: " +
                "ProcessedFileNames  <br />" +
                $"{string.Join(",<br /> ", _importState.ProcessedFileNames.OrderBy(name => name))} <br /> " +

                " <br />  <br /> Done FileNames <br />" +
                $"{string.Join(",<br /> ", _importState.DoneFileNames.OrderBy(name => name))} <br /> " +

                " <br />  <br /> Removed FileNames <br />" +
                $"{string.Join(",<br /> ", _importState.RemovedFileNames.OrderBy(name => name))} <br /> " +

                " <br />  <br /> Skipped FileNames <br />" +
                $"{string.Join(",<br /> ", _importState.SkippedFileNames.OrderBy(name => name))} <br /> " +
                " <br />  <br /> VirtSkipped FileNames <br />" +
                $"{string.Join(",<br /> ", _importState.VirtSkippedFileNames.OrderBy(name => name))} <br /> " +

                " <br />  <br /> UnknownShop FileNames <br />" +
                $"{string.Join(",<br /> ", _importState.UnknownShopFileNames.OrderBy(name => name))} <br /> "
                );

            if (_importState.ErrorAmount > 0)
            {
                log.Append($" ошибка в следующих файлах <br /> "
                            + $"{string.Join(",<br />", _importState.ErrorFileNames)} <br /> ");
            }

            _logger.LogWarning($"[1C export] {log}");


            var emails = Emails.Split(",").ToList();
            var messageBody = $"При импорте из 1С возникла ошибка в следующих файлах <br /> "
                        + $"{string.Join(",<br />", _importState.ErrorFileNames)} <br /> <br /> <br /> " +
                        $" {log}";
            var errorFiles = GetErrorFiles();
            emails.ForEach(email =>
            {
                _emailSender.SendEmail(email, "Отчет по 1С", messageBody, errorFiles);
            });
        }

        private IEnumerable<EmailAttachment> GetErrorFiles()
        {
            var list = new List<EmailAttachment>();
            foreach (var fileName in _importState.ErrorFileNames)
            {
                try
                {
                    string url = Path.Combine(FtpAddress, fileName);
                    byte[] data = _webClient.DownloadData(url);
                    list.Add(new EmailAttachment { Data = data, Name = fileName });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[1C][FromFTP] Error fhile preparing files for email");
                }
            }
            return list;
        }
    }
}