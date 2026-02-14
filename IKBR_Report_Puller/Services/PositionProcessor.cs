using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using IKBR_Report_Puller.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IKBR_Report_Puller.Services
{
    public class PositionProcessor
    {
        private readonly ITimeSeriesService _timeSeriesService;
        private readonly IDataService _dataService;
        private readonly IConfiguration _config;

        public PositionProcessor(ITimeSeriesService timeSeriesService, IDataService dataService, IConfiguration config)
        {
            _timeSeriesService = timeSeriesService;
            _dataService = dataService;
            _config = config;
        }

        public async Task ProcessPositionsAsync(IEnumerable<(string listingExchange, string symbol, string securityID)> positionDetails, dynamic mainReportXml)
        {
            foreach (var item in positionDetails)
            {
                Console.WriteLine($"Fetching data for security: {item.listingExchange + ":" + item.symbol + "(" + item.securityID + ")"}");

                string instrumentTicker = item.symbol;
                DateTime instrumentStartDate = DateTime.UtcNow.AddMonths(-1);
                DateTime instrumentEndDate = DateTime.UtcNow;
                string instrumentPeriod = "1d";

                string currency = ((IEnumerable<XElement>)mainReportXml.Descendants("OpenPosition"))
                                               .FirstOrDefault(op => op.Attribute("securityID")?.Value == item.securityID)?.Attribute("currency")?.Value ?? "USD";

                string instrumentTimeSeriesData = await _timeSeriesService.GetTimeSeriesDataAsync(instrumentTicker, item.listingExchange, instrumentStartDate, instrumentEndDate, instrumentPeriod);

                dynamic instrumentParsedData = JsonConvert.DeserializeObject(instrumentTimeSeriesData);
                var result = instrumentParsedData?.chart?.result?[0];
                if (result == null || result.indicators?.quote?[0] == null)
                {
                    Console.WriteLine($"No valid time series data found for {item.symbol}.");
                    continue;
                }

                var instrumentTimestamps = ((JArray)result.timestamp).ToObject<List<long>>();
                var instrumentQuotes = result.indicators.quote[0];

                if (instrumentTimestamps == null || instrumentQuotes.open == null)
                {
                    Console.WriteLine($"Incomplete time series data for {item.symbol}.");
                    continue;
                }

                if (result.timestamp == null)
                {
                    Console.WriteLine($"Timestamp data is missing for {item.symbol}.");
                    continue;
                }

                for (int i = 0; i < instrumentTimestamps.Count; i++)
                {
                    DateTime date = DateTimeOffset.FromUnixTimeSeconds(instrumentTimestamps[i]).DateTime;
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
    }
}