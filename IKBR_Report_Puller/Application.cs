using System;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Xml.Linq;
using IKBR_Report_Puller.Interfaces;
using IKBR_Report_Puller.Services;
using Microsoft.Extensions.Configuration;

namespace IKBR_Report_Puller
{
    public class Application
    {
        private readonly IReportFetchingService _reportFetchingService;
        private readonly IDataService _dataService;
        private readonly IExcelReportService _excelReportService;
        private readonly IConfiguration _config;
        private readonly ITimeSeriesService _timeSeriesService;
        private readonly PositionProcessor _positionProcessor;

        public Application(
            IReportFetchingService reportFetchingService,
            IDataService dataService,
            IExcelReportService excelReportService,
            IConfiguration config,
            ITimeSeriesService timeSeriesService)
        {
            _reportFetchingService = reportFetchingService;
            _dataService = dataService;
            _excelReportService = excelReportService;
            _config = config;
            _timeSeriesService = timeSeriesService;
            _positionProcessor = new PositionProcessor(_timeSeriesService, _dataService, _config);
        }

        public async Task RunAsync()
        {
            try
            {
                var outputFilePath = _config["IBKR:OutputFilePath"];
                const int maxRetries = 10;
                const int delayInSeconds = 15;
                (XDocument mainReportXml, string fileName) = await GetReportData(outputFilePath, maxRetries, delayInSeconds);
                SaveReportDataToDB(outputFilePath, mainReportXml);
                await WriteTodayReport(outputFilePath, maxRetries, delayInSeconds, fileName);

                //// Fetch instrument data for all open positions
                //var positionDetails = _dataService.GetOpenPositionInstrumentNames(mainReportXml)
                //    .Select(p => (p.listingExchange, p.symbol, p.securityID));
                //await _positionProcessor.ProcessPositionsAsync(positionDetails, mainReportXml);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
            }
        }

        private void SaveReportDataToDB(string? outputFilePath, XDocument mainReportXml)
        {
            _dataService.InsertTradeExecutions(mainReportXml);
            _dataService.InsertOpenPositions(mainReportXml);
            _excelReportService.CreateOpenPositionsReport(mainReportXml, outputFilePath);
        }

        private async Task<string> WriteTodayReport(string outputFilePath, int maxRetries, int delayInSeconds, string fileName)
        {
            //// Fetch and process today's report
            XDocument todayReportXml = await _reportFetchingService.FetchTodayReportAsync(maxRetries, delayInSeconds);
            fileName = DateTime.UtcNow.ToString("yyyyMMdd") + "_TraderSyncAccess_today.xml";
            string todayReportFilePath = outputFilePath.Replace("[FILE_NAME]", fileName);
            todayReportXml.Save(todayReportFilePath);
            Console.WriteLine($"Successfully saved 'Today' report to {todayReportFilePath}");

            _dataService.InsertTodayExecutions(todayReportXml);
            return fileName;
        }

        private async Task<(XDocument mainReportXml, string fileName)> GetReportData(string? outputFilePath, int maxRetries, int delayInSeconds)
        {
            //// Fetch and process main report
            XDocument mainReportXml = await _reportFetchingService.FetchMainReportAsync(maxRetries, delayInSeconds);
            var fileName = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "_TraderSyncAccess.xml";
            string mainReportFilePath = outputFilePath.Replace("[FILE_NAME]", fileName);
            mainReportXml.Save(mainReportFilePath);
            Console.WriteLine($"Successfully saved main report to {mainReportFilePath}");
            return (mainReportXml, fileName);
        }
    }
}
