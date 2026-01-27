using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using IKBR_Report_Puller.Interfaces;
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
        }

        public async Task RunAsync()
        {
            try
            {
                var outputFilePath = _config["IBKR:OutputFilePath"];
                const int maxRetries = 10;
                const int delayInSeconds = 15;

                //// Fetch and process main report
                XDocument mainReportXml = await _reportFetchingService.FetchMainReportAsync(maxRetries, delayInSeconds);
                string mainReportFilePath = outputFilePath.Replace("[FILE_NAME]", "TraderSyncAccess.xml");
                mainReportXml.Save(mainReportFilePath);
                Console.WriteLine($"Successfully saved main report to {mainReportFilePath}");

                _dataService.InsertTradeExecutions(mainReportXml);
                _dataService.InsertOpenPositions(mainReportXml);
                _excelReportService.CreateOpenPositionsReport(mainReportXml, outputFilePath);

                //// Fetch and process today's report
                XDocument todayReportXml = await _reportFetchingService.FetchTodayReportAsync(maxRetries, delayInSeconds);
                string todayReportFilePath = outputFilePath.Replace("[FILE_NAME]", "TraderSyncAccess_today.xml");
                todayReportXml.Save(todayReportFilePath);
                Console.WriteLine($"Successfully saved 'Today' report to {todayReportFilePath}");

                _dataService.InsertTodayExecutions(todayReportXml);

                // Fetch instrument data for all open positions
                var instrumentNames = _dataService.GetOpenPositionInstrumentNames();

                foreach (var instrument in instrumentNames)
                {
                    Console.WriteLine($"Fetching data for instrument: {instrument}");

                    string instrumentTicker = instrument; // Assuming instrument matches the ticker
                    DateTime instrumentStartDate = DateTime.UtcNow.AddMonths(-1);
                    DateTime instrumentEndDate = DateTime.UtcNow;
                    string instrumentPeriod = "1d";

                    string instrumentTimeSeriesData = await _timeSeriesService.GetTimeSeriesDataAsync(instrumentTicker, instrumentStartDate, instrumentEndDate, instrumentPeriod);
                    Console.WriteLine($"Time Series Data for {instrument}:");
                    Console.WriteLine(instrumentTimeSeriesData);

                    // Parse and save time series data
                    dynamic instrumentParsedData = Newtonsoft.Json.JsonConvert.DeserializeObject(instrumentTimeSeriesData);
                    var instrumentTimestamps = instrumentParsedData.chart.result[0].timestamp;
                    var instrumentQuotes = instrumentParsedData.chart.result[0].indicators.quote[0];

                    for (int i = 0; i < instrumentTimestamps.Count; i++)
                    {
                        DateTime date = DateTimeOffset.FromUnixTimeSeconds((long)instrumentTimestamps[i]).DateTime;
                        double open = instrumentQuotes.open[i];
                        double close = instrumentQuotes.close[i];
                        double low = instrumentQuotes.low[i];
                        double high = instrumentQuotes.high[i];
                        double volume = instrumentQuotes.volume[i];

                        _dataService.UpsertTimeSeriesData(
                            instrumentName: instrument,
                            provider: "YahooFinance",
                            dataName: "TimeSeries",
                            dataSource: "yfinance",
                            format: "JSON",
                            frequency: instrumentPeriod,
                            currency: "USD",
                            date: date,
                            openPrice: open,
                            closePrice: close,
                            lowPrice: low,
                            highPrice: high,
                            volume: volume
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
            }
        }
    }
}
