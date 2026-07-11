using IKBR_Report_Puller.Domain;
using Microsoft.Extensions.Configuration;
using PikUpStix.TraderView.Interfaces;
using System.Xml.Linq;

namespace IKBR_Report_Puller.Services
{
    public class ReportRunnerService : IReportRunnerService
    {
        private readonly IReportFetchingService _reportFetchingService;
        private readonly ITradeExecutionRepository _tradeExecutionRepository;
        private readonly IInstrumentRepository _instrumentRepository;
        private readonly IOpenPositionRepository _openPositionRepository;
        private readonly IExcelReportService _excelReportService;
        private readonly IConfiguration _config;
        private readonly ITradeHistoryReportService _tradeHistoryReportService;
        private readonly IMarketDataService marketDataService;
        const int maxRetries = 3;
        const int delayInSeconds = 5;
        string outputFilePath = @"C:\IBKR_Reports\[FILE_NAME]";
        public ReportRunnerService(
            IReportFetchingService reportFetchingService,
            ITradeExecutionRepository tradeExecutionRepository,
            IInstrumentRepository instrumentRepository,
            IOpenPositionRepository openPositionRepository,
            IExcelReportService excelReportService,
            ITradeHistoryReportService tradeHistoryReportService,
            IMarketDataService economicCalendarService,
            IConfiguration config)
        {
            _reportFetchingService = reportFetchingService;
            _tradeExecutionRepository = tradeExecutionRepository;
            _instrumentRepository = instrumentRepository;
            _openPositionRepository = openPositionRepository;
            _excelReportService = excelReportService;
            _tradeHistoryReportService = tradeHistoryReportService;
            marketDataService = economicCalendarService;
            _config = config;
            outputFilePath = _config["IBKR:OutputFilePath"];
        }
        public async Task RunReportAsync()
        {
            try
            {
                (IKBRReport mainReport, string fileName) = await GetReportDataFromInteractiveBrokers();
                // Upsert instruments first, then trade executions, then open positions
                _instrumentRepository.UpsertInstruments(mainReport.Trades, marketDataService.SourceName);
                _tradeExecutionRepository.UpsertTradeExecutions(mainReport.Trades);
                _openPositionRepository.InsertOpenPositions(mainReport.WhenGenerated, mainReport.OpenPositions);
                var executions = _tradeExecutionRepository.GetTradeExecutions();
                _tradeHistoryReportService.CreateTradeHistoryReport(executions);
                _excelReportService.CreateExcelFileReport(mainReport.OpenPositions, executions, outputFilePath);

                await WriteTodayReport(fileName);
                await marketDataService.FetchAndSaveChartData(_tradeHistoryReportService.TradeHistoryAggregated);
                await marketDataService.FetchAndSaveEconomicCalendarAsync(DateTime.Now.AddDays(-30), DateTime.Now.AddDays(30));
                await marketDataService.FetchAndSaveChartData(new List<string>()
                    {
                        "^GSPC",//spx
                        "^RUT",//iwm
                        //"CLUSD",//wti crude oil
                        "BTCUSD",//bitcoin
                        "GCUSD",//gold
                        "XAGUSD",//silver
                        "QQQ",//nasdaq
                        "^VIX"
                     }, 300);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"\nAn error occurred: {ex.Message}");
            }
        }
        private async Task<string> WriteTodayReport(string fileName) 
        {
            //// Fetch and process today's report
            XDocument todayReportXml = await _reportFetchingService.FetchTodayReportAsync(maxRetries, delayInSeconds);
            fileName = DateTime.UtcNow.ToString("yyyyMMdd") + "_TraderSyncAccess_today.xml";
            string todayReportFilePath = outputFilePath.Replace("[FILE_NAME]", fileName);

            // Ensure directory exists
            string directory = Path.GetDirectoryName(todayReportFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                System.Console.WriteLine($"Created directory: {directory}");
            }

            todayReportXml.Save(todayReportFilePath);
            System.Console.WriteLine($"Successfully saved 'Today' report to {todayReportFilePath}");

            // Convert XDocument to IKBRReport
            var todayReport = IKBRReportParser.ParseTodayReport(todayReportXml);

            // Upsert instruments first, then trade executions
            _instrumentRepository.UpsertInstruments(todayReport.TradeConfirms, marketDataService.SourceName);
            _tradeExecutionRepository.UpsertTodayExecutions(todayReport.TradeConfirms);

            return fileName;
        }

        private async Task<(IKBRReport mainReport, string fileName)> GetReportDataFromInteractiveBrokers()
        {
            //// Fetch and process main report
            //XDocument mainReportXml = LoadXmlDocument("C:\\Users\\finn\\OneDrive\\Documents\\Wealth\\Business\\trading\\TradeExecution Diaries\\20260708_190402_TraderSyncAccess.xml");
            XDocument mainReportXml = await _reportFetchingService.FetchMainReportAsync(maxRetries, delayInSeconds);
            var fileName = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "_TraderSyncAccess.xml";
            string mainReportFilePath = outputFilePath.Replace("[FILE_NAME]", fileName);

            // Ensure directory exists
            string directory = Path.GetDirectoryName(mainReportFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                System.Console.WriteLine($"Created directory: {directory}");
            }

            mainReportXml.Save(mainReportFilePath);
            System.Console.WriteLine($"Successfully saved main report to {mainReportFilePath}");

            // Convert XDocument to IKBRReport
            var mainReport = IKBRReportParser.ParseMainReport(mainReportXml);

            return (mainReport, fileName);
        }
        public static XDocument LoadXmlDocument(string directory)
        {
            try
            {
                // Simple validation to ensure the file actually exists
                if (!File.Exists(directory))
                {
                    System.Console.WriteLine($"Error: File not found at {directory}");
                    return null;
                }

                // XDocument.Load handles the heavy lifting
                return XDocument.Load(directory);
            }
            catch (Exception ex)
            {
                // Handles XML parsing errors, permissions, etc.
                System.Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }
    }
}