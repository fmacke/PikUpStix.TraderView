using System;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Xml.Linq;
using IKBR_Report_Puller.Domain;
using IKBR_Report_Puller.IKBR;
using IKBR_Report_Puller.Interfaces;
using IKBR_Report_Puller.Services;
using Microsoft.Extensions.Configuration;
using PikUpStix.TraderView.Interfaces;

namespace IKBR_Report_Puller.Console
{
    public class Application
    {
        private readonly IReportFetchingService _reportFetchingService;
        private readonly ITradeExecutionRepository _tradeExecutionRepository;
        private readonly IInstrumentRepository _instrumentRepository;
        private readonly IOpenPositionRepository _openPositionRepository;
        private readonly IExcelReportService _excelReportService;
        private readonly IConfiguration _config;
        private readonly IHistoricalDataService _historicalDataService;
        private readonly ITradeHistoryReportService _tradeHistoryReportService;

        const int maxRetries = 10;
        const int delayInSeconds = 10;
        string outputFilePath = @"C:\IBKR_Reports\[FILE_NAME]";

        public Application(
            IReportFetchingService reportFetchingService,
            ITradeExecutionRepository tradeExecutionRepository,
            IInstrumentRepository instrumentRepository,
            IOpenPositionRepository openPositionRepository,
            IExcelReportService excelReportService,
            IHistoricalDataService historicalDataService,
            ITradeHistoryReportService tradeHistoryReportService,
            IConfiguration config)
        {
            _reportFetchingService = reportFetchingService;
            _tradeExecutionRepository = tradeExecutionRepository;
            _instrumentRepository = instrumentRepository;
            _openPositionRepository = openPositionRepository;
            _excelReportService = excelReportService;
            _historicalDataService = historicalDataService;
            _tradeHistoryReportService = tradeHistoryReportService;
            _config = config;
            outputFilePath = _config["IBKR:OutputFilePath"];
        }

        public async Task RunAsync()
        {
            try
            {
                (IKBRReport mainReport, string fileName) = await GetReportData();
                await SaveReportDataToDB(mainReport);
                await WriteTodayReport(fileName);
                //var marketDataToUpdate = new List<Instrument>()
                //{
                //    new Instrument( )
                //    {
                //        InstrumentName = "SPX",
                //        Provider = "IBKR",
                //        DataName = "SPX",
                //        DataSource = "IBKR",
                //        Format = "PullerUpdate",
                //        Frequency = "D1",
                //        ContractUnitType = "INDEX",
                //        PriceQuotation = "USD",
                //        Currency = "USD",
                //        ListingExchange = "CBOE",
                //        ConId = "416904"
                //    },
                //};
                //await _historicalDataService.UpdateHistoricalDataForInstruments(marketDataToUpdate, DateTime.Now.AddDays(-200), DateTime.Now);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"\nAn error occurred: {ex.Message}");
            }
        }

        private async Task SaveReportDataToDB(IKBRReport mainReport)
        {
            // Upsert instruments first, then trade executions (order matters due to FK constraints)
            _instrumentRepository.UpsertInstruments(mainReport.Trades);
            _tradeExecutionRepository.UpsertTradeExecutions(mainReport.Trades);
            _openPositionRepository.InsertOpenPositions(mainReport.WhenGenerated, mainReport.OpenPositions);
            

            _excelReportService.CreateReport(mainReport, outputFilePath);
            _tradeHistoryReportService.CreateTradeHistoryReport(_tradeExecutionRepository.GetTradeExecutions());
            await _historicalDataService.UpdateHistoricalDataForOpenPositions(mainReport.OpenPositions);
            await _historicalDataService.UpdateHistoricalDataForHistoricalTrades(_tradeHistoryReportService.TradeHistoryAggregated);       
        }

        private async Task<string> WriteTodayReport(string fileName)
        {
            //// Fetch and process today's report
            XDocument todayReportXml = await _reportFetchingService.FetchTodayReportAsync(maxRetries, delayInSeconds);
            fileName = DateTime.UtcNow.ToString("yyyyMMdd") + "_TraderSyncAccess_today.xml";
            string todayReportFilePath = outputFilePath.Replace("[FILE_NAME]", fileName);
            todayReportXml.Save(todayReportFilePath);
            System.Console.WriteLine($"Successfully saved 'Today' report to {todayReportFilePath}");

            // Convert XDocument to IKBRReport
            var todayReport = IKBRReportParser.ParseTodayReport(todayReportXml);

            // Upsert instruments first, then trade executions
            _instrumentRepository.UpsertInstruments(todayReport.TradeConfirms);
            _tradeExecutionRepository.UpsertTodayExecutions(todayReport.TradeConfirms);

            return fileName;
        }

        private async Task<(IKBRReport mainReport, string fileName)> GetReportData()
        {
            //// Fetch and process main report
            XDocument mainReportXml = await _reportFetchingService.FetchMainReportAsync(maxRetries, delayInSeconds);
            var fileName = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "_TraderSyncAccess.xml";
            string mainReportFilePath = outputFilePath.Replace("[FILE_NAME]", fileName);
            mainReportXml.Save(mainReportFilePath);
            System.Console.WriteLine($"Successfully saved main report to {mainReportFilePath}");

            // Convert XDocument to IKBRReport
            var mainReport = IKBRReportParser.ParseMainReport(mainReportXml);

            return (mainReport, fileName);
        }
    }
}
