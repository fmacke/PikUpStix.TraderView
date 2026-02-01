using System;
using System.IO;
using System.Security.Principal;
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
                var fileName = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "_TraderSyncAccess.xml";   
                string mainReportFilePath = outputFilePath.Replace("[FILE_NAME]", fileName);
                mainReportXml.Save(mainReportFilePath);
                Console.WriteLine($"Successfully saved main report to {mainReportFilePath}");

                _dataService.InsertTradeExecutions(mainReportXml);
                _dataService.InsertOpenPositions(mainReportXml);
                _excelReportService.CreateOpenPositionsReport(mainReportXml, outputFilePath);

                //// Fetch and process today's report
                XDocument todayReportXml = await _reportFetchingService.FetchTodayReportAsync(maxRetries, delayInSeconds);
                fileName = DateTime.UtcNow.ToString("yyyyMMdd") + "_TraderSyncAccess_today.xml";
                string todayReportFilePath = outputFilePath.Replace("[FILE_NAME]", fileName);
                todayReportXml.Save(todayReportFilePath);
                Console.WriteLine($"Successfully saved 'Today' report to {todayReportFilePath}");

                _dataService.InsertTodayExecutions(todayReportXml);

                // Fetch instrument data for all open positions
                var positionDetails = _dataService.GetOpenPositionInstrumentNames(mainReportXml);

                foreach (var item in positionDetails)
                {
                    Console.WriteLine($"Fetching data for security: {item.listingExchange + ":" + item.symbol + "(" + item.securityID + ")"}");

                    string instrumentTicker = item.symbol; // Assuming instrument matches the ticker
                    DateTime instrumentStartDate = DateTime.UtcNow.AddMonths(-1);
                    DateTime instrumentEndDate = DateTime.UtcNow;
                    string instrumentPeriod = "1d";

                    // Dynamically set currency from the XML report data
                    string currency = mainReportXml.Descendants("OpenPosition")
                                                   .FirstOrDefault(op => op.Attribute("securityID")?.Value == item.securityID)?.Attribute("currency")?.Value ?? "USD";

                    string instrumentTimeSeriesData = await _timeSeriesService.GetTimeSeriesDataAsync(instrumentTicker, item.listingExchange, instrumentStartDate, instrumentEndDate, instrumentPeriod);
                    Console.WriteLine($"Time Series Data for {item.symbol}:");
                    Console.WriteLine(instrumentTimeSeriesData);

                    // Parse and validate time series data
                    dynamic instrumentParsedData = Newtonsoft.Json.JsonConvert.DeserializeObject(instrumentTimeSeriesData);
                    var result = instrumentParsedData?.chart?.result?[0];
                    if (result == null || result.indicators?.quote?[0] == null)
                    {
                        Console.WriteLine($"No valid time series data found for {item.symbol}.");
                        continue;
                    }

                    var instrumentTimestamps = result.timestamp;
                    var instrumentQuotes = result.indicators.quote[0];

                    if (instrumentTimestamps == null || instrumentQuotes.open == null)
                    {
                        Console.WriteLine($"Incomplete time series data for {item.symbol}.");
                        continue;
                    }

                    for (int i = 0; i < instrumentTimestamps.Count; i++)
                    {
                        DateTime date = DateTimeOffset.FromUnixTimeSeconds((long)instrumentTimestamps[i]).DateTime;
                        double open = instrumentQuotes.open[i];
                        double close = instrumentQuotes.close[i];
                        double low = instrumentQuotes.low[i];
                        double high = instrumentQuotes.high[i];
                        double volume = instrumentQuotes.volume[i];

                        _dataService.UpsertTimeSeriesData(
                            instrumentName: item.symbol,
                            listingExchange: item.listingExchange,
                            securityIdentifier: item.securityID,
                            provider: "YahooFinance",
                            dataName: "TimeSeries",
                            dataSource: "yfinance",
                            format: "JSON",
                            frequency: instrumentPeriod,
                            currency: currency,
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
