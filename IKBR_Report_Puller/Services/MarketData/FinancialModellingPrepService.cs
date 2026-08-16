using TraderView.Domain.Entities;
using System.Net.Http.Json;
using System.Text.Json;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Models.FMP;

namespace PikUpStix.TraderView.Services.MarketData
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
        async Task<List<EconomicCalendarEvent>> IMarketDataService.FetchAndSaveEconomicCalendarAsync(DateTime fromDate, DateTime toDate)
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

        async Task IMarketDataService.FetchAndSaveChartData(List<HistoricalTrade> trades)
        {
            foreach (var trade in trades)
            {

                var instrument = await _instrumentRepository.GetByIdAsync(trade.InstrumentId);
                await ExecuteWithErrorHandlingAsync(async () =>
                {
                    var fromDate = trade.TradeOpened.AddDays(-364);
                    var toDate = trade.TradeClosed.AddDays(364);
                    if (toDate > DateTime.UtcNow)
                    {
                        toDate = DateTime.UtcNow.AddDays(-1); // don't pull today's data as it's not complete
                    }

                    var barData = await FetchChartDataFromApiAsync(instrument.DataSource, fromDate, toDate);

                    if (barData == null || barData.Count == 0)
                    {
                        Console.WriteLine("No chart data found for the specified date range.");
                        return;
                    }

                    Console.WriteLine($"Retrieved {barData.Count} rows of chart data for {trade.Symbol}.");

                    _historicalDataRepository.UpdateHistoricalData(trade.InstrumentId.ToString(), barData);
                }, $"Symbol: {trade.Symbol}, InstrumentId: {trade.InstrumentId}");
            }
        }

        async Task IMarketDataService.FetchAndSaveChartData(List<string> symbols, int lookBackDays)
        {
            foreach (var symbol in symbols)
            {
                await ExecuteWithErrorHandlingAsync(async () =>
                {
                    var fromDate = DateTime.Now.AddDays(lookBackDays * -1);
                    var toDate = DateTime.Now;
                    if (toDate > DateTime.UtcNow)
                    {
                        toDate = DateTime.UtcNow;
                    }
                    var instrumentId = _instrumentRepository.GetInstrumentIdByConId(symbol);
                    if (instrumentId == null)
                    {
                        throw new Exception($"No instrument in database for symbol {symbol}.  Skipping for now.");
                        //instrumentId = _instrumentRepository.InsertInstrument(symbol, symbol, "FinancialModellingPrep", "USD", "INDEX", "FinancialModellingPrep", symbol);
                    }
                    var instrument = _instrumentRepository.Get(instrumentId.Value);
                    var barData = await FetchChartDataFromApiAsync(instrument.DataName, fromDate, toDate);

                    if (barData == null || barData.Count == 0)
                    {
                        Console.WriteLine("No chart data found for the specified date range.");
                        return;
                    }

                    Console.WriteLine($"Retrieved {barData.Count} rows of chart data for {symbol}.");

                    _historicalDataRepository.UpdateHistoricalData(instrumentId.ToString(), barData);
                }, $"Symbol: {symbol}");
            }
        }

        async Task IMarketDataService.FetchLatestPrices(List<Position> positions)
        {
            foreach (var position in positions)
            {
                await ExecuteWithErrorHandlingAsync(async () =>
                {
                    var barData = await FetchChartDataFromApiAsync(position.Instrument.DataName, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow);
                    if (barData == null || barData.Count == 0)
                    {
                        Console.WriteLine($"No latest price data found for {position.Instrument.DataName}.");
                        return;
                    }
                    var latestBar = barData.OrderByDescending(b => b.Date).FirstOrDefault();
                    if (latestBar != null)
                    {
                        Console.WriteLine($"Updated latest price for {position.Instrument.DataName}: {latestBar.ClosePrice}");
                        position.LastReportedPriceUpdated = DateTime.UtcNow;
                        position.LastReportedPrice = (decimal)latestBar.ClosePrice;
                    }
                }, $"InstrumentId: {position.Id}, Symbol: {position.Instrument.DataName}");
            }
        }
        public async Task<IReadOnlyList<FmpQuarterlyIncomeStatementDto>> GetQuarterlyIncomeStatementsAsync(
            string symbol,
            int limit = 8,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"v3/income-statement/{symbol.ToUpperInvariant()}?period=quarter&limit={limit}&apikey={_apiKey}";
                var result = await _httpClient.GetFromJsonAsync<List<FmpQuarterlyIncomeStatementDto>>(url, cancellationToken);

                return result ?? (IReadOnlyList<FmpQuarterlyIncomeStatementDto>)Array.Empty<FmpQuarterlyIncomeStatementDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching quarterly income statements for {Symbol} with error {Error}", symbol, ex.Message);
                return Array.Empty<FmpQuarterlyIncomeStatementDto>();
            }
        }

        public async Task<CanSlimCurrentQuarterMetric?> EvaluateCurrentQuarterEpsAsync(
            string symbol,
            decimal minEpsGrowth = 25m,
            decimal minRevenueGrowth = 20m,
            CancellationToken cancellationToken = default)
        {
            // Fetch at least 8 quarters to evaluate YoY growth across consecutive recent quarters
            var statements = await GetQuarterlyIncomeStatementsAsync(symbol, 8, cancellationToken);

            if (statements == null || statements.Count < 5)
            {
                Console.WriteLine("Insufficient quarterly history for CAN SLIM 'C' evaluation on {Symbol}", symbol);
                return null;
            }

            // Statements are returned latest first [Q0, Q-1, Q-2, Q-3, Q-4 (YoY for Q0), Q-5 (YoY for Q-1), ...]
            var currentQ = statements[0];
            var priorYearQ = statements[4];

            // Calculate Latest YoY Growth
            var epsGrowthYoY = CalculatePercentageGrowth(priorYearQ.EpsDiluted, currentQ.EpsDiluted);
            var revGrowthYoY = CalculatePercentageGrowth(priorYearQ.Revenue, currentQ.Revenue);

            // Check Acceleration (Compare Q0 YoY vs Q-1 YoY)
            bool isAccelerating = false;
            if (statements.Count >= 6)
            {
                var prevQ = statements[1];
                var prevPriorYearQ = statements[5];
                var prevEpsGrowthYoY = CalculatePercentageGrowth(prevPriorYearQ.EpsDiluted, prevQ.EpsDiluted);
                isAccelerating = epsGrowthYoY > prevEpsGrowthYoY;
            }

            return new CanSlimCurrentQuarterMetric
            {
                Symbol = symbol.ToUpperInvariant(),
                LatestQuarterDate = currentQ.Date,
                LatestQuarterEps = currentQ.EpsDiluted,
                PriorYearQuarterEps = priorYearQ.EpsDiluted,
                EpsGrowthYoYPercent = Math.Round(epsGrowthYoY, 2),
                RevenueGrowthYoYPercent = Math.Round(revGrowthYoY, 2),
                IsAccelerating = isAccelerating,
                PassesCriteria = epsGrowthYoY >= minEpsGrowth && revGrowthYoY >= minRevenueGrowth
            };
        }

        private static decimal CalculatePercentageGrowth(decimal baseValue, decimal currentValue)
        {
            if (baseValue == 0)
            {
                return currentValue > 0 ? 100m : 0m;
            }

            // Handles negative base EPS turning profitable or standard growth
            return ((currentValue - baseValue) / Math.Abs(baseValue)) * 100m;
        }
    
        /// <summary>
        /// Executes an async operation with standardized error handling
        /// </summary>
        private static async Task ExecuteWithErrorHandlingAsync(Func<Task> operation, string context = null)
        {
            try
            {
                await operation();
            }
            catch (HttpRequestException ex)
            {
                var contextInfo = !string.IsNullOrEmpty(context) ? $" [{context}]" : "";
                Console.WriteLine($"HTTP error fetching data{contextInfo}: {ex.Message}");
                throw new HttpRequestException($"HTTP error fetching data{contextInfo}: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                var contextInfo = !string.IsNullOrEmpty(context) ? $" [{context}]" : "";
                Console.WriteLine($"JSON deserialization error{contextInfo}: {ex.Message}");
                throw new JsonException($"JSON deserialization error{contextInfo}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                var contextInfo = !string.IsNullOrEmpty(context) ? $" [{context}]" : "";
                Console.WriteLine($"Error fetching and saving FMP{contextInfo}: {ex.Message}");
                throw new Exception($"Error fetching and saving FMP{contextInfo}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Normalizes a symbol by removing special characters for API requests
        /// </summary>
        private static string NormalizeSymbol(string symbol)
        {
            return symbol.Replace("/", "").Replace("-", "").Replace(" ", "");//.Replace(".", "");
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
