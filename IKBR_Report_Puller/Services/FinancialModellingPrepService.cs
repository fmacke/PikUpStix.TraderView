using IKBR_Report_Puller.Data.Repositories;
using IKBR_Report_Puller.Domain;
using PikUpStix.TraderView.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace IKBR_Report_Puller.Services
{
    /// <summary>
    /// Service for retrieving economic calendar data from Financial Modeling Prep API
    /// </summary>
    public class FinancialModellingPrepService : IMarketDataService
    {
        private readonly HttpClient _httpClient;
        private readonly IEconomicCalendarRepository _repository;
        private readonly IHistoricalDataRepository _historicalDataRepository;
        private readonly IInstrumentRepository _instrumentRepository;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _outputFilePath;

        public string SourceName => "FinancialModellingPrep";

        public FinancialModellingPrepService(
            HttpClient httpClient,
            IEconomicCalendarRepository repository,
            IHistoricalDataRepository historicalDataRepository,
            IInstrumentRepository instrumentRepository,
            string apiKey,
            string baseUrl,
            string outputFilePath)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _historicalDataRepository = historicalDataRepository ?? throw new ArgumentNullException(nameof(historicalDataRepository));
            _instrumentRepository = instrumentRepository ?? throw new ArgumentNullException(nameof(instrumentRepository));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _outputFilePath = outputFilePath ?? throw new ArgumentNullException(nameof(outputFilePath));
        }

        /// <summary>
        /// Fetches economic calendar data from API, saves to file and database
        /// </summary>
        public async Task<List<EconomicCalendarEvent>> FetchAndSaveEconomicCalendarAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                // Build API URL with date parameters
                var fromDateStr = fromDate.ToString("yyyy-MM-dd");
                var toDateStr = toDate.ToString("yyyy-MM-dd");
                var url = $"{_baseUrl}/economic-calendar?from={fromDateStr}&to={toDateStr}&apikey={_apiKey}";
                Console.WriteLine($"Fetching economic calendar data from {fromDateStr} to {toDateStr}...");

                // Fetch data from API
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                // Deserialize JSON response
                var events = JsonSerializer.Deserialize<List<EconomicCalendarEvent>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (events == null || events.Count == 0)
                {
                    Console.WriteLine("No economic calendar barData found for the specified date range.");
                    return new List<EconomicCalendarEvent>();
                }

                Console.WriteLine($"Retrieved {events.Count} economic calendar barData.");

                // Save to file
                await SaveToFileAsync(events, fromDateStr, toDateStr);

                // Save to database
                _repository.UpsertEconomicCalendarEvents(events);

                return events;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP error fetching economic calendar: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON deserialization error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching and saving economic calendar: {ex.Message}");
                throw;
            }
        }

        public async Task FetchAndSaveChartData(List<HistoricalTrade> trades)
        {
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                foreach (var trade in trades)
                {
                    var fromDate = trade.TradeOpened.AddDays(-200);
                    var toDate = trade.TradeClosed.AddDays(200);
                    if (toDate > DateTime.UtcNow)
                    {
                        toDate = DateTime.UtcNow;
                    }

                    var barData = await FetchChartDataFromApiAsync(trade.Symbol, fromDate, toDate);

                    if (barData == null || barData.Count == 0)
                    {
                        Console.WriteLine("No chart data found for the specified date range.");
                        continue;
                    }

                    Console.WriteLine($"Retrieved {barData.Count} rows of chart data for {trade.Symbol}.");

                    _historicalDataRepository.UpdateHistoricalData(trade.InstrumentId.ToString(), barData);
                }
            });
        }

        public async Task FetchAndSaveChartData(List<string> symbols, int lookBackDays)
        {
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                foreach (var symbol in symbols)
                {
                    var fromDate = DateTime.Now.AddDays(lookBackDays * -1);
                    var toDate = DateTime.Now;
                    if (toDate > DateTime.UtcNow)
                    {
                        toDate = DateTime.UtcNow;
                    }
                    var instrumentId = _instrumentRepository.GetInstrumentIdFromConId(symbol);
                    if (instrumentId == null)
                    {
                        Console.WriteLine($"No instrument found for symbol {symbol} so adding to database.");
                        instrumentId = _instrumentRepository.InsertInstrument(symbol, symbol, "FinancialModellingPrep", "USD", "INDEX", "FinancialModellingPrep", "FinancialModellingPrep");
                    }
                    var instrument = _instrumentRepository.Get(instrumentId.Value);
                    var barData = await FetchChartDataFromApiAsync(instrument.DataName, fromDate, toDate);

                    if (barData == null || barData.Count == 0)
                    {
                        Console.WriteLine("No chart data found for the specified date range.");
                        continue;
                    }

                    Console.WriteLine($"Retrieved {barData.Count} rows of chart data for {symbol}.");

                    _historicalDataRepository.UpdateHistoricalData(instrumentId.ToString(), barData);
                }
            });
        }

        /// <summary>
        /// Executes an async operation with standardized error handling
        /// </summary>
        private static async Task ExecuteWithErrorHandlingAsync(Func<Task> operation)
        {
            try
            {
                await operation();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP error fetching economic calendar: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON deserialization error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching and saving FMP: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Normalizes a symbol by removing special characters for API requests
        /// </summary>
        private static string NormalizeSymbol(string symbol)
        {
            return symbol.Replace("/", "").Replace("-", "").Replace(" ", "").Replace(".", "");
        }

        /// <summary>
        /// Fetches chart data from the API for a given symbol and date range
        /// </summary>
        private async Task<List<Bar>> FetchChartDataFromApiAsync(string symbol, DateTime fromDate, DateTime toDate)
        {

            var fromDateStr = fromDate.ToString("yyyy-MM-dd");
            var toDateStr = toDate.ToString("yyyy-MM-dd");
            var normalizedSymbol = NormalizeSymbol(symbol);
            var url = $"{_baseUrl}/historical-price-eod/full?symbol={normalizedSymbol}&from={fromDateStr}&to={toDateStr}&apikey={_apiKey}";

            Console.WriteLine($"Fetching time series data from {fromDateStr} to {toDateStr}...");

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var barData = JsonSerializer.Deserialize<List<Bar>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return barData ?? new List<Bar>();
        }

        /// <summary>
        /// Saves economic calendar barData to a JSON file
        /// </summary>
        private async Task SaveToFileAsync(List<EconomicCalendarEvent> events, string fromDate, string toDate)
        {
            try
            {
                // Ensure directory exists
                Directory.CreateDirectory(_outputFilePath);

                // Create filename with date range
                var fileName = $"EconomicCalendar_{fromDate}_to_{toDate}_{DateTime.UtcNow:yyyyMMddHHmmss}.json";
                var filePath = Path.Combine(_outputFilePath, fileName);

                // Serialize and save to file
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(events, jsonOptions);
                await File.WriteAllTextAsync(filePath, json);

                Console.WriteLine($"Economic calendar data saved to: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving economic calendar to file: {ex.Message}");
                throw;
            }
        }
    }
}
