using IKBR_Report_Puller.Data.Repositories;
using IKBR_Report_Puller.Domain;
using IKBR_Report_Puller.Interfaces;
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
    public class FinancialModellingPrepService : IEconomicDataService
    {
        private readonly HttpClient _httpClient;
        private readonly IEconomicCalendarRepository _repository;
        private readonly IHistoricalDataRepository _historicalDataRepository;
        private readonly IInstrumentRepository _instrumentRepository;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _outputFilePath;

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
            try
            {
                foreach (var trade in trades)
                {                
                    // Build API URL with date parameters
                    var fromDateStr = trade.TradeOpened.AddDays(-200).ToString("yyyy-MM-dd");
                    var toDate = trade.TradeClosed.AddDays(200);
                    if(toDate > DateTime.UtcNow)
                    {
                        toDate = DateTime.UtcNow;
                    }
                    var symbol = trade.Symbol.Replace("/", "").Replace("-", "").Replace(" ", "").Replace(".", "");
                    var toDateStr = toDate.ToString("yyyy-MM-dd");
                    var url = $"{_baseUrl}/historical-price-eod/full?symbol={symbol}&from={fromDateStr}&to={toDateStr}&apikey={_apiKey}";
                    Console.WriteLine($"Fetching time series data from {fromDateStr} to {toDateStr}...");

                    // Fetch data from API
                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();

                    // Deserialize JSON response
                    var barData = JsonSerializer.Deserialize<List<Bar>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (barData == null || barData.Count == 0)
                    {
                        Console.WriteLine("No chart data found for the specified date range.");
                    }

                    Console.WriteLine($"Retrieved {barData.Count} rows of chart data for {trade.Symbol}.");

                    // Save to database
                    _historicalDataRepository.UpdateHistoricalData(trade.InstrumentId.ToString(), barData);
                }
                
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

        public async Task FetchAndSaveChartData(List<string> symbols, int lookBackDays)
        {
            try
            {
                foreach (var symbol in symbols)
                {
                    // Build API URL with date parameters
                    var fromDateStr = DateTime.Now.AddDays(lookBackDays*-1).ToString("yyyy-MM-dd");
                    var toDate = DateTime.Now;
                    if (toDate > DateTime.UtcNow)
                    {
                        toDate = DateTime.UtcNow;
                    }
                    var sym = symbol.Replace("/", "").Replace("-", "").Replace(" ", "").Replace(".", "");
                    var toDateStr = toDate.ToString("yyyy-MM-dd");
                    var url = $"{_baseUrl}/historical-price-eod/full?symbol={sym}&from={fromDateStr}&to={toDateStr}&apikey={_apiKey}";
                    Console.WriteLine($"Fetching time series data from {fromDateStr} to {toDateStr}...");

                    // Fetch data from API
                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();

                    // Deserialize JSON response
                    var barData = JsonSerializer.Deserialize<List<Bar>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (barData == null || barData.Count == 0)
                    {
                        Console.WriteLine("No chart data found for the specified date range.");
                    }

                    Console.WriteLine($"Retrieved {barData.Count} rows of chart data for {symbol}.");

                    var instrumentId = _instrumentRepository.GetInstrumentIdFromConId(symbol);
                    if(instrumentId == null)
                    {
                        Console.WriteLine($"No instrument found for symbol {symbol} so adding to database.");
                        instrumentId = _instrumentRepository.InsertInstrument(symbol, symbol, "FMP","USD", "INDEX","FinancialModellingPrep", "FinancialModellingPrep");
                    }
                    // Save to database
                    _historicalDataRepository.UpdateHistoricalData(instrumentId.ToString(), barData);
                }

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
