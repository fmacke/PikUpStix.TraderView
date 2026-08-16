//using TraderView.Domain.Entities;
//using System.Text.Json;
//using System.Text.Json.Serialization;
//using TraderView.Application.Interfaces.Repositories;
//using TraderView.Application.Interfaces.Services;
//using TraderView.Application.Models.FMP;

//namespace PikUpStix.TraderView.Services.MarketData
//{
//    /// <summary>
//    /// Service for retrieving market data from Yahoo Finance API
//    /// </summary>
//    public class YahooFinanceService : IMarketDataService
//    {
//        private readonly HttpClient _httpClient;
//        private readonly IEconomicCalendarRepository _repository;
//        private readonly IHistoricalDataRepository _historicalDataRepository;
//        private readonly IInstrumentRepository _instrumentRepository;
//        private readonly string _baseUrl;
//        private readonly string _outputFilePath;

//        public string SourceName => "YahooFinance";

//        string IMarketDataService.SourceName => throw new NotImplementedException();

//        public YahooFinanceService(
//            HttpClient httpClient,
//            IEconomicCalendarRepository repository,
//            IHistoricalDataRepository historicalDataRepository,
//            IInstrumentRepository instrumentRepository,
//            string baseUrl,
//            string outputFilePath)
//        {
//            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
//            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
//            _historicalDataRepository = historicalDataRepository ?? throw new ArgumentNullException(nameof(historicalDataRepository));
//            _instrumentRepository = instrumentRepository ?? throw new ArgumentNullException(nameof(instrumentRepository));
//            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
//            _outputFilePath = outputFilePath ?? throw new ArgumentNullException(nameof(outputFilePath));
//        }

//        /// <summary>
//        /// Fetches economic calendar data - Yahoo Finance doesn't provide economic calendar directly,
//        /// so this implementation returns empty list as placeholder
//        /// </summary>
//        public async Task<List<EconomicCalendarEvent>> FetchAndSaveEconomicCalendarAsync(DateTime fromDate, DateTime toDate)
//        {
//            try
//            {
//                Console.WriteLine($"Yahoo Finance does not provide economic calendar data. Returning empty list.");

//                // Yahoo Finance doesn't have a dedicated economic calendar endpoint
//                // This would require either:
//                // 1. Using a different service for economic calendar
//                // 2. Web scraping Yahoo Finance's economic calendar page
//                // 3. Using a third-party Yahoo Finance wrapper that includes this data

//                var events = new List<EconomicCalendarEvent>();

//                // Save to database (empty list)
//                _repository.UpsertEconomicCalendarEvents(events);

//                return events;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error in Yahoo Finance economic calendar: {ex.Message}");
//                throw;
//            }
//        }

//        /// <summary>
//        /// Fetches and saves chart data for a list of historical trades
//        /// </summary>
//        public async Task FetchAndSaveChartData(List<HistoricalTrade> trades)
//        {
//            foreach (var trade in trades)
//            {
//                await ExecuteWithErrorHandlingAsync(async () =>
//                {
//                    var fromDate = trade.TradeOpened.AddDays(-200);
//                    var toDate = trade.TradeClosed.AddDays(200);
//                    if (toDate > DateTime.UtcNow)
//                    {
//                        toDate = DateTime.UtcNow.AddDays(-1); // don't pull today's data as it's not complete
//                    }

//                    var barData = await FetchChartDataFromApiAsync(trade.Symbol, fromDate, toDate);

//                    if (barData == null || barData.Count == 0)
//                    {
//                        Console.WriteLine("No chart data found for the specified date range.");
//                        return;
//                    }

//                    Console.WriteLine($"Retrieved {barData.Count} rows of chart data for {trade.Symbol}.");

//                    _historicalDataRepository.UpdateHistoricalData(trade.InstrumentId.ToString(), barData);
//                }, $"Symbol: {trade.Symbol}, InstrumentId: {trade.InstrumentId}");
//            }
//        }

//        /// <summary>
//        /// Fetches and saves chart data for a list of symbols
//        /// </summary>
//        public async Task FetchAndSaveChartData(List<string> symbols, int lookBackDays)
//        {
//            foreach (var symbol in symbols)
//            {
//                await ExecuteWithErrorHandlingAsync(async () =>
//                {
//                    var fromDate = DateTime.Now.AddDays(lookBackDays * -1);
//                    var toDate = DateTime.Now;
//                    if (toDate > DateTime.UtcNow)
//                    {
//                        toDate = DateTime.UtcNow;
//                    }

//                    var instrumentId = _instrumentRepository.GetInstrumentIdByConId(symbol);
//                    if (instrumentId == null)
//                    {
//                        Console.WriteLine($"No instrument found for symbol {symbol} so adding to database.");
//                        instrumentId = _instrumentRepository.InsertInstrument(symbol, symbol, "YahooFinance", "USD", "STOCK", "YahooFinance", "YahooFinance");
//                    }

//                    var instrument = _instrumentRepository.Get(instrumentId.Value);
//                    var barData = await FetchChartDataFromApiAsync(instrument.DataName, fromDate, toDate);

//                    if (barData == null || barData.Count == 0)
//                    {
//                        Console.WriteLine("No chart data found for the specified date range.");
//                        return;
//                    }

//                    Console.WriteLine($"Retrieved {barData.Count} rows of chart data for {symbol}.");

//                    _historicalDataRepository.UpdateHistoricalData(instrumentId.ToString(), barData);
//                }, $"Symbol: {symbol}");
//            }
//        }

//        /// <summary>
//        /// Fetches chart data from Yahoo Finance API for a given symbol and date range
//        /// Uses Yahoo Finance v8 API endpoints
//        /// </summary>
//        private async Task<List<Bar>> FetchChartDataFromApiAsync(string symbol, DateTime fromDate, DateTime toDate)
//        {
//            var bars = new List<Bar>();
//            try
//            {
//                // Convert dates to Unix timestamps (Yahoo Finance uses Unix timestamps)
//                var fromUnix = ((DateTimeOffset)fromDate).ToUnixTimeSeconds();
//                var toUnix = ((DateTimeOffset)toDate).ToUnixTimeSeconds();

//                var normalizedSymbol = NormalizeSymbol(symbol);

//                // Yahoo Finance API v8 endpoint for historical data
//                // Using daily interval (1d)
//                var url = $"{_baseUrl}/v8/finance/chart/{normalizedSymbol}?period1={fromUnix}&period2={toUnix}&interval=1d";

//                Console.WriteLine($"Fetching Yahoo Finance data for {normalizedSymbol} from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}...");

//                var response = await _httpClient.GetAsync(url);
//                response.EnsureSuccessStatusCode();

//                var content = await response.Content.ReadAsStringAsync();

//                // Parse Yahoo Finance response structure
//                var yahooResponse = JsonSerializer.Deserialize<YahooFinanceResponse>(content, new JsonSerializerOptions
//                {
//                    PropertyNameCaseInsensitive = true
//                });

//                if (yahooResponse?.Chart?.Result == null || yahooResponse.Chart.Result.Count == 0)
//                {
//                    Console.WriteLine($"No data returned from Yahoo Finance for {normalizedSymbol}");
//                    return new List<Bar>();
//                }

//                var result = yahooResponse.Chart.Result[0];
//                var timestamps = result.Timestamp;
//                var quotes = result.Indicators?.Quote?[0];

//                if (timestamps == null || quotes == null)
//                {
//                    Console.WriteLine($"Invalid data structure from Yahoo Finance for {normalizedSymbol}");
//                    return new List<Bar>();
//                }

//                // Convert Yahoo Finance data to Bar objects
                
//                for (int i = 0; i < timestamps.Count; i++)
//                {
//                    // Skip bars with null data
//                    if (quotes.Open[i] == null || quotes.Close[i] == null || 
//                        quotes.High[i] == null || quotes.Low[i] == null)
//                    {
//                        continue;
//                    }

//                    bars.Add(new Bar
//                    {
//                        Date = DateTimeOffset.FromUnixTimeSeconds(timestamps[i]).UtcDateTime,
//                        OpenPrice = quotes.Open[i] ?? 0,
//                        ClosePrice = quotes.Close[i] ?? 0,
//                        HighPrice = quotes.High[i] ?? 0,
//                        LowPrice = quotes.Low[i] ?? 0,
//                        Volume = quotes.Volume[i] ?? 0,
//                        Settle = 0,
//                        OpenInterest = 0
//                    });
//                }

//                Console.WriteLine($"Parsed {bars.Count} bars from Yahoo Finance data");
//                return bars;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error fetching data from Yahoo Finance: {ex.Message}");
//            }
//            return bars;
//        }

//        /// <summary>
//        /// Executes an async operation with standardized error handling
//        /// </summary>
//        private static async Task ExecuteWithErrorHandlingAsync(Func<Task> operation, string context = null)
//        {
//            try
//            {
//                await operation();
//            }
//            catch (HttpRequestException ex)
//            {
//                var contextInfo = !string.IsNullOrEmpty(context) ? $" [{context}]" : "";
//                Console.WriteLine($"HTTP error fetching data{contextInfo}: {ex.Message}");
//                throw new HttpRequestException($"HTTP error fetching data{contextInfo}: {ex.Message}", ex);
//            }
//            catch (JsonException ex)
//            {
//                var contextInfo = !string.IsNullOrEmpty(context) ? $" [{context}]" : "";
//                Console.WriteLine($"JSON deserialization error{contextInfo}: {ex.Message}");
//                throw new JsonException($"JSON deserialization error{contextInfo}: {ex.Message}", ex);
//            }
//            catch (Exception ex)
//            {
//                var contextInfo = !string.IsNullOrEmpty(context) ? $" [{context}]" : "";
//                Console.WriteLine($"Error fetching and saving Yahoo Finance data{contextInfo}: {ex.Message}");
//                throw new Exception($"Error fetching and saving Yahoo Finance data{contextInfo}: {ex.Message}", ex);
//            }
//        }

//        /// <summary>
//        /// Normalizes a symbol for Yahoo Finance (most symbols work as-is, but some need adjustments)
//        /// </summary>
//        private static string NormalizeSymbol(string symbol)
//        {
//            // Yahoo Finance typically uses the symbol as-is, but you may need to adjust for:
//            // - Indices (e.g., ^GSPC for S&P 500, ^DJI for Dow Jones)
//            // - International stocks (e.g., append exchange like .L for London)
//            return symbol.Trim();
//        }

//        Task IMarketDataService.FetchLatestPrices(List<Position> positions)
//        {
//            throw new NotImplementedException();
//        }

//        Task<List<EconomicCalendarEvent>> IMarketDataService.FetchAndSaveEconomicCalendarAsync(DateTime fromDate, DateTime toDate)
//        {
//            throw new NotImplementedException();
//        }

//        Task IMarketDataService.FetchAndSaveChartData(List<HistoricalTrade> trades)
//        {
//            throw new NotImplementedException();
//        }

//        Task IMarketDataService.FetchAndSaveChartData(List<string> symbols, int lookBackDays)
//        {
//            throw new NotImplementedException();
//        }

//        Task<IReadOnlyList<FmpQuarterlyIncomeStatementDto>> IMarketDataService.GetQuarterlyIncomeStatementsAsync(string symbol, int limit)
//        {
//            throw new NotImplementedException();
//        }

//        Task<CanSlimCurrentQuarterMetric?> IMarketDataService.EvaluateCurrentQuarterEpsAsync(string symbol, decimal minEpsGrowth, decimal minRevenueGrowth)
//        {
//            throw new NotImplementedException();
//        }

//        #region Yahoo Finance Response DTOs

//        private class YahooFinanceResponse
//        {
//            [JsonPropertyName("chart")]
//            public ChartData Chart { get; set; }
//        }

//        private class ChartData
//        {
//            [JsonPropertyName("result")]
//            public List<ChartResult> Result { get; set; }

//            [JsonPropertyName("error")]
//            public object Error { get; set; }
//        }

//        private class ChartResult
//        {
//            [JsonPropertyName("meta")]
//            public MetaData Meta { get; set; }

//            [JsonPropertyName("timestamp")]
//            public List<long> Timestamp { get; set; }

//            [JsonPropertyName("indicators")]
//            public Indicators Indicators { get; set; }
//        }

//        private class MetaData
//        {
//            [JsonPropertyName("currency")]
//            public string Currency { get; set; }

//            [JsonPropertyName("symbol")]
//            public string Symbol { get; set; }

//            [JsonPropertyName("exchangeName")]
//            public string ExchangeName { get; set; }

//            [JsonPropertyName("instrumentType")]
//            public string InstrumentType { get; set; }
//        }

//        private class Indicators
//        {
//            [JsonPropertyName("quote")]
//            public List<QuoteData> Quote { get; set; }

//            [JsonPropertyName("adjclose")]
//            public List<AdjCloseData> AdjClose { get; set; }
//        }

//        private class QuoteData
//        {
//            [JsonPropertyName("open")]
//            public List<double?> Open { get; set; }

//            [JsonPropertyName("close")]
//            public List<double?> Close { get; set; }

//            [JsonPropertyName("high")]
//            public List<double?> High { get; set; }

//            [JsonPropertyName("low")]
//            public List<double?> Low { get; set; }

//            [JsonPropertyName("volume")]
//            public List<double?> Volume { get; set; }
//        }

//        private class AdjCloseData
//        {
//            [JsonPropertyName("adjclose")]
//            public List<double?> AdjClose { get; set; }
//        }

//        #endregion
//    }
//}
