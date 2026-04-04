using System;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Xml.Linq;
using IKBR_Report_Puller.Domain;
using IKBR_Report_Puller.Interfaces;
using IKBR_Report_Puller.Services;
using Microsoft.Extensions.Configuration;

namespace IKBR_Report_Puller
{
    public class Application
    {
        private readonly IReportFetchingService _reportFetchingService;
        private readonly IChartDataService _chartDataService;
        private readonly IDataService _dataService;
        private readonly IExcelReportService _excelReportService;
        private readonly IConfiguration _config;

        const int maxRetries = 10;
        const int delayInSeconds = 2;
        string outputFilePath = @"C:\IBKR_Reports\[FILE_NAME]";

        public Application(
            IReportFetchingService reportFetchingService,
            IChartDataService chartService,
            IDataService dataService,
            IExcelReportService excelReportService,
            IConfiguration config)
        {
            _reportFetchingService = reportFetchingService;
            _dataService = dataService;
            _excelReportService = excelReportService;
            _chartDataService = chartService;            
            _config = config;
            outputFilePath = _config["IBKR:OutputFilePath"];
        }

        public async Task RunAsync()
        {
            try
            {
                (IKBRReport mainReport, string fileName) = await GetReportData();
                SaveReportDataToDB(mainReport);
                await WriteTodayReport(fileName);                
                SaveChartDataForTrades(mainReport);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
            }
        }

        private void SaveChartDataForTrades(IKBRReport report)
        {
            var trades = _dataService.GetTradeExecutions();
            if (!trades.Any())
            {
                Console.WriteLine("No trade confirmations found in the report.");
                return;
            }

            foreach (var trade in trades)
            {
                var openTime = trade.TradeDate.AddDays(-10);
                var closeTime = trade.TradeDate.AddDays(10);

                // Check if chart data exists for the trade's open-to-close time range
                //bool chartDataExists = _chartDataService.ChartDataExistsForTimeRange(openTime, closeTime);

                //if (!chartDataExists)
                //{
                //    Console.WriteLine($"Missing chart data for trade ID {trade.IbOrderID} from {openTime} to {closeTime}.");
                //}
                //else
                //{
                //    Console.WriteLine($"Chart data exists for trade ID {trade.IbOrderID}.");
                //}
            }
        }

        private void SaveReportDataToDB(IKBRReport mainReport)
        {
            _dataService.InsertTradeExecutions(mainReport);
            _dataService.InsertOpenPositions(mainReport);
            _excelReportService.CreateReport(mainReport, outputFilePath);
        }

        private async Task<string> WriteTodayReport(string fileName)
        {
            //// Fetch and process today's report
            XDocument todayReportXml = await _reportFetchingService.FetchTodayReportAsync(maxRetries, delayInSeconds);
            fileName = DateTime.UtcNow.ToString("yyyyMMdd") + "_TraderSyncAccess_today.xml";
            string todayReportFilePath = outputFilePath.Replace("[FILE_NAME]", fileName);
            todayReportXml.Save(todayReportFilePath);
            Console.WriteLine($"Successfully saved 'Today' report to {todayReportFilePath}");

            // Convert XDocument to IKBRReport
            var todayReport = IKBRReportParser.ParseTodayReport(todayReportXml);
            _dataService.InsertTodayExecutions(todayReport);

            return fileName;
        }

        private async Task<(IKBRReport mainReport, string fileName)> GetReportData()
        {
            //// Fetch and process main report
            XDocument mainReportXml = await _reportFetchingService.FetchMainReportAsync(maxRetries, delayInSeconds);
            var fileName = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "_TraderSyncAccess.xml";
            string mainReportFilePath = outputFilePath.Replace("[FILE_NAME]", fileName);
            mainReportXml.Save(mainReportFilePath);
            Console.WriteLine($"Successfully saved main report to {mainReportFilePath}");

            // Convert XDocument to IKBRReport
            var mainReport = IKBRReportParser.ParseMainReport(mainReportXml);

            return (mainReport, fileName);
        }
    }
}
