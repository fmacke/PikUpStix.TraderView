using System;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Xml.Linq;
using IKBR_Report_Puller.Domain;
using IKBR_Report_Puller.Helpers;
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
        private readonly ITradeHistoryReportService _tradeHistoryReportService;

        const int maxRetries = 10;
        const int delayInSeconds = 2;
        string outputFilePath = @"C:\IBKR_Reports\[FILE_NAME]";

        public Application(
            IReportFetchingService reportFetchingService,
            IChartDataService chartService,
            IDataService dataService,
            IExcelReportService excelReportService,
            ITradeHistoryReportService tradeHistoryReportService,
            IConfiguration config)
        {
            _reportFetchingService = reportFetchingService;
            _dataService = dataService;
            _excelReportService = excelReportService;
            _chartDataService = chartService;   
            _chartDataService.ConnectAsync(config["IBKRClient:SocketUrl"], int.Parse(config["IBKRClient:Port"]), int.Parse(config["IBKRClient:ClientId"])).Wait();
            _config = config;
            _tradeHistoryReportService = tradeHistoryReportService;
            outputFilePath = _config["IBKR:OutputFilePath"];
        }

        public async Task RunAsync()
        {
            try
            {
                (IKBRReport mainReport, string fileName) = await GetReportData();
                SaveReportDataToDB(mainReport);
                await WriteTodayReport(fileName);                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
            }
        }

        private void SaveReportDataToDB(IKBRReport mainReport)
        {
            _dataService.InsertTradeExecutions(mainReport);
            _dataService.InsertOpenPositions(mainReport);
            _excelReportService.CreateReport(mainReport, outputFilePath);
            UpdateHistoricalDataForPositions();
        }

        private void UpdateHistoricalDataForPositions()
        {
            _tradeHistoryReportService.CreateTradeHistoryReport(_dataService.GetTradeExecutions());
            var trades = _tradeHistoryReportService.TradeHistoryAggregated;

            foreach (var trade in trades)
            {
                // Calculate date range: startDate - 100 days to endDate + 20 days
                DateTime requiredStartDate = trade.TradeOpened.AddDays(-100);
                DateTime requiredEndDate = trade.TradeClosed.AddDays(20);

                // If endDate + 20 is past today, use today instead
                DateTime today = DateTime.Today;
                if (requiredEndDate > today)
                {
                    requiredEndDate = today;
                }

                Console.WriteLine($"Checking historical data for {trade.Symbol} from {requiredStartDate:yyyy-MM-dd} to {requiredEndDate:yyyy-MM-dd}");

               // Check for missing date ranges
                var missingRanges = _dataService.GetMissingDateRanges(trade.InstrumentId, requiredStartDate, requiredEndDate);

                if (missingRanges.Any())
                {
                    Console.WriteLine($"Found {missingRanges.Count} missing date range(s) for {trade.Symbol}:");
                    foreach (var range in missingRanges)
                    {
                        Console.WriteLine($"  Missing data from {range.startDate:yyyy-MM-dd} to {range.endDate:yyyy-MM-dd}");
                        FetchHistoricalDataStub(trade.SecurityId, trade.Symbol, range.startDate, range.endDate, trade.InstrumentId);
                    }
                }
                else
                {
                    Console.WriteLine($"All required historical data exists for {trade.Symbol}");
                }
            }
        }

        /// <summary>
        /// Fetches historical data from IBKR and saves it to the database
        /// </summary>
        private async void FetchHistoricalDataStub(string conid, string symbol, DateTime startDate, DateTime endDate, int instrumentId)
        {
            try
            {
                Console.WriteLine($"Fetching historical data for {symbol} (conid: {conid}) from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                // Fetch data from IBKR API
                var ibkrBars = await _chartDataService.GetHistoricalDataAsync(conid, startDate, endDate);

                if (ibkrBars == null || !ibkrBars.Any())
                {
                    Console.WriteLine($"Warning: No historical data returned for {symbol}");
                    return;
                }

                // Convert IBKR bars to domain bars
                var domainBars = BarConverter.ConvertToDomainBars(ibkrBars, instrumentId);

                // Save to database
                _dataService.UpsertHistoricalData(instrumentId.ToString(), domainBars);

                Console.WriteLine($"Successfully saved {domainBars.Count} bars for {symbol}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching historical data for {symbol}: {ex.Message}");
            }
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
