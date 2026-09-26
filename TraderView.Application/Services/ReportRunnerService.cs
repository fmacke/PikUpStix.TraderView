using Microsoft.Extensions.Configuration;
using TraderView.Domain.Entities;
using PikUpStix.TraderView.Interfaces;
using PikUpStix.TraderView.Services.MarketData;
using System.Text;
using System.Xml.Linq;

namespace PikUpStix.TraderView.Services
{
    public class ReportRunnerService : IReportRunnerService
    {
        private readonly IReportFetchingService _reportFetchingService;
        private readonly ITradeExecutionRepository _tradeExecutionRepository;
        private readonly IPositionRepository _positionRepository;
        private readonly IInstrumentRepository _instrumentRepository;
        private readonly IExcelReportService _excelReportService;
        private readonly IConfiguration _config;
        private readonly ITradeHistoryReportService _tradeHistoryReportService;
        private readonly IMarketDataService _marketDataService;
        private readonly YahooFinanceService _yahooFinanceService;
        private readonly FinancialModellingPrepService _fmpService;
        const int maxRetries = 3;
        const int delayInSeconds = 5;
        string outputFilePath = @"C:\IBKR_Reports\[FILE_NAME]";
        public ReportRunnerService(
            IReportFetchingService reportFetchingService,
            ITradeExecutionRepository tradeExecutionRepository,
            IPositionRepository positionRepository,
            IInstrumentRepository instrumentRepository,
            IExcelReportService excelReportService,
            ITradeHistoryReportService tradeHistoryReportService,
            IMarketDataService economicCalendarService,
            YahooFinanceService yahooFinanceService,
            FinancialModellingPrepService fmpService,
            IConfiguration config)
        {
            _reportFetchingService = reportFetchingService;
            _tradeExecutionRepository = tradeExecutionRepository;
            _positionRepository = positionRepository;
            _instrumentRepository = instrumentRepository;
            _excelReportService = excelReportService;
            _tradeHistoryReportService = tradeHistoryReportService;
            _marketDataService = economicCalendarService;
            _yahooFinanceService = yahooFinanceService;
            _fmpService = fmpService;
            _config = config;
            outputFilePath = _config["IBKR:OutputFilePath"];
        }
        public async Task RunReportAsync(bool writeOutputtoExcel, bool updateMarketData)
        {
            try
            {
                (IKBRReport mainReport, string fileName) = await GetReportDataFromInteractiveBrokers();
                _instrumentRepository.UpsertInstruments(mainReport.Trades, _marketDataService.SourceName);
                await _tradeExecutionRepository.UpsertTradeExecutionsAsync(mainReport.Trades);
                await UpdateOpenPositionPrices();
                var executions = await _tradeExecutionRepository.GetTradeExecutionsAsync();

                XDocument todayReportXml = await _reportFetchingService.FetchTodayReportAsync(maxRetries, delayInSeconds);
                await SaveTradeConfirms(todayReportXml);

                if (writeOutputtoExcel)
                {
                    var openPositions = await _positionRepository.GetOpenPositionsAsync();
                    await _excelReportService.CreateExcelFileReport(openPositions, executions, outputFilePath);
                    await WriteTodayReportToExcel(todayReportXml);
                }
                if (updateMarketData)
                {
                    _tradeHistoryReportService.CreateTradeHistoryReport(executions);
                    await ((IMarketDataService)_fmpService).FetchAndSaveChartData(_tradeHistoryReportService.TradeHistoryAggregated);

                    await _marketDataService.FetchAndSaveEconomicCalendarAsync(DateTime.Now.AddDays(-30), DateTime.Now.AddDays(30));
                    await _marketDataService.FetchAndSaveChartData(new List<string>()
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
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"\nAn error occurred: {ex.Message}");
            }
        }

        private async Task SaveTradeConfirms(XDocument todayReportXml)
        {
            // Convert XDocument to IKBRReport
            var todayReport = IKBRReportParser.ParseTodayReport(todayReportXml);

            // Insert instruments first, then trade confirmations
            _instrumentRepository.UpsertInstruments(todayReport.TradeConfirms, _marketDataService.SourceName);
            await _tradeExecutionRepository.InsertTradeConfirmationsAsync(todayReport.TradeConfirms);
        }

        private async Task UpdateOpenPositionPrices()
        {
            var openPositions = await _positionRepository.GetOpenPositionsAsync();
            await _marketDataService.FetchLatestPrices(openPositions);
            await _positionRepository.UpsertPositionsAsync(openPositions);
        }

        private async Task<string> WriteTodayReportToExcel(XDocument todayReportXml) 
        {
            string fileName = DateTime.UtcNow.ToString("yyyyMMdd") + "_TraderSyncAccess_today.xml";
            StringBuilder todayReportFilePath = new StringBuilder(outputFilePath).Append("\\" + fileName);

            // Ensure directory exists
            string directory = System.IO.Path.GetDirectoryName(todayReportFilePath.ToString());
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                System.Console.WriteLine($"Created directory: {directory}");
            }

            todayReportXml.Save(todayReportFilePath.ToString());
            System.Console.WriteLine($"Successfully saved 'Today' report to {todayReportFilePath}");            

            return fileName;
        }

        private async Task<(IKBRReport mainReport, string fileName)> GetReportDataFromInteractiveBrokers()
        {
            // Fetch and process main report
            //XDocument mainReportXml = LoadXmlDocument("C:\\Users\\finn\\OneDrive\\Documents\\Wealth\\Business\\trading\\Trade Diaries\\20260812_215421_TraderSyncAccess.xml");
            XDocument mainReportXml = await _reportFetchingService.FetchMainReportAsync(maxRetries, delayInSeconds);
            var fileName = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "_TraderSyncAccess.xml";
            StringBuilder mainReportFilePath = new StringBuilder(outputFilePath).Append("\\" + fileName);

            // Ensure directory exists
            string directory = System.IO.Path.GetDirectoryName(mainReportFilePath.ToString());
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                System.Console.WriteLine($"Created directory: {directory}");
            }

            mainReportXml.Save(mainReportFilePath.ToString());
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