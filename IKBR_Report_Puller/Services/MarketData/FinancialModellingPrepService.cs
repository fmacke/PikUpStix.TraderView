using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Models.FMP;
using TraderView.Domain.Entities;

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
                    var barData = await FetchChartDataFromApiAsync(position.Instrument.DataSource, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow);
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
            int limit = 8)
        {
            try
            {
                var url = $"{_baseUrl}/income-statement?symbol={symbol.ToUpperInvariant()}&period=quarter&limit={limit}&apikey={_apiKey}";
                var result = await _httpClient.GetFromJsonAsync<List<FmpQuarterlyIncomeStatementDto>>(url);

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
            decimal minRevenueGrowth = 20m)
        {
            // Fetch at least 8 quarters to evaluate YoY growth across consecutive recent quarters
            var statements = await GetQuarterlyIncomeStatementsAsync(symbol, 8);

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
        public async Task<CanSlimAnnualMetric?> EvaluateAnnualEpsAsync(string symbol, decimal minCagr = 25m, decimal minRoe = 17m)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                throw new ArgumentException("Ticker symbol cannot be null or whitespace.", nameof(symbol));
            }

            var cleanSymbol = symbol.Trim().ToUpperInvariant();

            // 1. Concurrently fetch 5 years of annual income statements and TTM key metrics
            var annualsTask = GetAnnualIncomeStatementsAsync(cleanSymbol, limit: 5);
            var metricsTask = GetKeyMetricsTtmAsync(cleanSymbol);

            await Task.WhenAll(annualsTask, metricsTask);

            var annuals = annualsTask.Result;
            var metrics = metricsTask.Result;

            // CAN SLIM 'A' requires at least 4 consecutive completed fiscal years (Y0, Y-1, Y-2, Y-3)
            if (annuals == null || annuals.Count < 4)
            {
                Console.WriteLine("Insufficient annual statement history for CAN SLIM 'A' evaluation on {0} (Found: {1}, Required: 4+)",
                    cleanSymbol, annuals?.Count ?? 0);
                return null;
            }

            // FMP returns annual statements sorted latest first:
            // index 0 = Y0 (latest fiscal year), 1 = Y-1, 2 = Y-2, 3 = Y-3 (3 years prior)
            var y0 = annuals[0].EpsDiluted;
            var y1 = annuals[1].EpsDiluted;
            var y2 = annuals[2].EpsDiluted;
            var y3 = annuals[3].EpsDiluted;

            // 2. Monotonic Annual EPS Progression Check (Y0 > Y1 > Y2)
            // O'Neil requires consistent earnings growth without major cyclical breakdowns
            bool hasConsecutiveGrowth = (y0 > y1) && (y1 > y2);

            // 3. Compute 3-Year EPS Compound Annual Growth Rate (CAGR)
            // Standard Formula: (Y0 / Y3)^(1/3) - 1
            decimal cagr3YearPercent = 0m;
            if (y3 > 0 && y0 > 0)
            {
                double ratio = (double)(y0 / y3);
                double cagr = Math.Pow(ratio, 1.0 / 3.0) - 1.0;
                cagr3YearPercent = Math.Round((decimal)(cagr * 100.0), 2);
            }
            else if (y3 <= 0 && y0 > 0)
            {
                // Turnaround exception (negative EPS 3 years ago turned solidly profitable)
                decimal divisor = Math.Abs(y3 == 0m ? 0.01m : y3);
                cagr3YearPercent = Math.Round(((y0 - y3) / divisor) * 100m, 2);
            }

            // 4. Optional 5-Year EPS CAGR Calculation
            decimal? cagr5YearPercent = null;
            if (annuals.Count >= 5)
            {
                var y4 = annuals[4].EpsDiluted;
                if (y4 > 0 && y0 > 0)
                {
                    double ratio5 = (double)(y0 / y4);
                    double cagr5 = Math.Pow(ratio5, 1.0 / 4.0) - 1.0;
                    cagr5YearPercent = Math.Round((decimal)(cagr5 * 100.0), 2);
                }
            }

            // 5. Extract TTM Return on Equity (ROE) & Margins from Key Metrics
            decimal returnOnEquity = 0m;
            decimal operatingMargin = 0m;
            decimal returnOnAssets = 0m;

            if (metrics != null && metrics.Count > 0)
            {
                var primaryMetric = metrics[0];
                returnOnEquity = Math.Round(primaryMetric.Roe * 100m, 2);
                operatingMargin = Math.Round(primaryMetric.OperatingProfitMargin * 100m, 2);
                returnOnAssets = Math.Round(primaryMetric.Roa * 100m, 2);
            }

            // 6. Build Historical Annual Earnings Progression Points (for charting / audit breakdown)
            var history = new List<AnnualEarningsPoint>();
            for (int i = 0; i < annuals.Count; i++)
            {
                decimal yoyGrowth = 0m;
                if (i + 1 < annuals.Count)
                {
                    var current = annuals[i].EpsDiluted;
                    var prior = annuals[i + 1].EpsDiluted;
                    yoyGrowth = CalculatePercentageGrowth(prior, current);
                }

                history.Add(new AnnualEarningsPoint
                {
                    CalendarYear = annuals[i].CalendarYear,
                    FiscalDate = annuals[i].Date,
                    Revenue = annuals[i].Revenue,
                    NetIncome = annuals[i].NetIncome,
                    EpsDiluted = annuals[i].EpsDiluted,
                    EpsGrowthYoYPercent = Math.Round(yoyGrowth, 2)
                });
            }

            // 7. CAN SLIM 'A' Strict Pass/Fail Gate
            bool passesCriteria = cagr3YearPercent >= minCagr &&
                                  returnOnEquity >= minRoe &&
                                  hasConsecutiveGrowth;

            return new CanSlimAnnualMetric
            {
                Symbol = cleanSymbol,
                EvaluationDateUtc = DateTime.UtcNow,
                EpsCagr3YearPercent = cagr3YearPercent,
                EpsCagr5YearPercent = cagr5YearPercent,
                ReturnOnEquityPercent = returnOnEquity,
                HasConsecutiveAnnualGrowth = hasConsecutiveGrowth,
                LatestFiscalYearEps = y0,
                LatestFiscalYear = annuals[0].CalendarYear,
                PriorYear1Eps = y1,
                PriorYear2Eps = y2,
                PriorYear3Eps = y3,
                OperatingMarginPercent = operatingMargin,
                ReturnOnAssetsPercent = returnOnAssets,
                AnnualHistory = history,
                PassesCriteria = passesCriteria
            };
        }
        public async Task<IReadOnlyList<FmpKeyMetricsDto>> GetKeyMetricsTtmAsync(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                throw new ArgumentException("Ticker symbol cannot be null or whitespace.", nameof(symbol));
            }

            var cleanSymbol = symbol.Trim().ToUpperInvariant();

            try
            {
                // FMP TTM Key Metrics endpoint
                var url = $"{_baseUrl}/key-metrics-ttm?symbol={cleanSymbol}&apikey={_apiKey}";

                var result = await _httpClient.GetFromJsonAsync<List<FmpKeyMetricsDto>>(url);

                if (result == null || result.Count == 0)
                {
                    Console.WriteLine($"No TTM key metrics returned from FMP for {cleanSymbol}");
                    return Array.Empty<FmpKeyMetricsDto>();
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP error fetching TTM key metrics for {cleanSymbol} from FMP (Status: {ex.StatusCode})");
                return Array.Empty<FmpKeyMetricsDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error fetching TTM key metrics for {cleanSymbol}: {ex.Message}");
                return Array.Empty<FmpKeyMetricsDto>();
            }
        }
        public async Task<IReadOnlyList<FmpAnnualIncomeStatementDto>> GetAnnualIncomeStatementsAsync(string symbol, int limit = 5)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                throw new ArgumentException("Ticker symbol cannot be null or whitespace.", nameof(symbol));
            }

            var cleanSymbol = symbol.Trim().ToUpperInvariant();

            try
            {
                // FMP endpoint for annual statements defaults to period=annual, but explicitly passing it guarantees correct grouping
                var url = $"{_baseUrl}/income-statement/?symbol={cleanSymbol}&period=annual&limit={limit}&apikey={_apiKey}";

                var result = await _httpClient.GetFromJsonAsync<List<FmpAnnualIncomeStatementDto>>(url);

                if (result == null || result.Count == 0)
                {
                    Console.WriteLine($"No annual income statements returned from FMP for {cleanSymbol}");
                    return Array.Empty<FmpAnnualIncomeStatementDto>();
                }

                // Ensure returned statements are ordered newest to oldest (Y0 down to Y-4)
                return result
                    .OrderByDescending(x => x.CalendarYear)
                    .ThenByDescending(x => x.Date)
                    .ToList();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP error occurred while fetching annual income statements for {cleanSymbol} from FMP (Status: {ex.StatusCode}): {ex.Message}");
                return Array.Empty<FmpAnnualIncomeStatementDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error fetching annual income statements for {cleanSymbol}: {ex.Message}");
                return Array.Empty<FmpAnnualIncomeStatementDto>();
            }
        }
        public async Task<IReadOnlyList<CanSlimCandidate>> RunScreenerAsync(CanSlimScreenerCriteria criteria)
        {
            // STAGE 1: Bulk screener API call to fetch liquid universe
            var url = $"{_baseUrl}/company-screener?priceMoreThan={criteria.MinPrice}&volumeMoreThan={criteria.MinVolume}&marketCapMoreThan={criteria.MinMarketCap}&isEtf=false&isActivelyTrading=true&exchange=NASDAQ,NYSE&country=US&limit=600&apikey={_apiKey}";

            var preFiltered = await _httpClient.GetFromJsonAsync<List<FmpScreenerResultDto>>(url);
            if (preFiltered == null || preFiltered.Count == 0)
            {
                return Array.Empty<CanSlimCandidate>();
            }

            Console.WriteLine($"Stage 1 Pre-Filter passed {preFiltered.Count} candidates. Running Stage 2 & 3 deep evaluations...");

            var passedCandidates = new ConcurrentBag<CanSlimCandidate>();
            var throttler = new SemaphoreSlim(criteria.MaxDegreeOfParallelism);

            // STAGE 3: Parallel evaluation of 'C' and 'A'
            var tasks = preFiltered.Select(async stock =>
            {
                await throttler.WaitAsync();
                try
                {
                    var caResult = await EvaluateCanSlimCAAsync(stock.Symbol);

                    if (caResult != null &&
                        caResult.PassesBoth &&
                        caResult.CurrentQuarter != null &&
                        caResult.Annual != null &&
                        caResult.CurrentQuarter.EpsGrowthYoYPercent >= criteria.MinCurrentQuarterEpsGrowthPercent &&
                        caResult.CurrentQuarter.RevenueGrowthYoYPercent >= criteria.MinCurrentQuarterRevGrowthPercent &&
                        caResult.Annual.EpsCagr3YearPercent >= criteria.MinAnnualEpsCagrPercent &&
                        caResult.Annual.ReturnOnEquityPercent >= criteria.MinReturnOnEquityPercent)
                    {
                        passedCandidates.Add(new CanSlimCandidate
                        {
                            Symbol = stock.Symbol,
                            CompanyName = stock.CompanyName,
                            Sector = stock.Sector,
                            Industry = stock.Industry,
                            Price = stock.Price,
                            Volume = stock.Volume,
                            MarketCap = stock.MarketCap,
                            CurrentQuarter = caResult.CurrentQuarter,
                            Annual = caResult.Annual
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed evaluating CAN SLIM criteria for {stock.Symbol}: {ex}");
                }
                finally
                {
                    throttler.Release();
                }
            });

            await Task.WhenAll(tasks);

            return passedCandidates
                .OrderByDescending(x => x.CurrentQuarter.EpsGrowthYoYPercent)
                .ToList();
        }
        public async Task<CanSlimEvaluationResult> EvaluateCanSlimCAAsync(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                throw new ArgumentException("Ticker symbol cannot be null or whitespace.", nameof(symbol));
            }

            var cleanSymbol = symbol.Trim().ToUpperInvariant();

            // 1. Run 'C' (Quarterly) and 'A' (Annual) evaluations concurrently to minimize API latency
            var currentQuarterTask = EvaluateCurrentQuarterEpsAsync(cleanSymbol);
            var annualTask = EvaluateAnnualEpsAsync(cleanSymbol);

            await Task.WhenAll(currentQuarterTask, annualTask);

            var currentQuarter = currentQuarterTask.Result;
            var annual = annualTask.Result;

            // 2. Validate availability of data
            if (currentQuarter == null || annual == null)
            {
                Console.WriteLine($"Incomplete data returned for CAN SLIM C+A evaluation on {cleanSymbol}");

                return new CanSlimEvaluationResult
                {
                    Symbol = cleanSymbol,
                    CurrentQuarter = currentQuarter,
                    Annual = annual,
                    PassesBoth = false
                };
            }

            // 3. Evaluate composite O'Neil CAN SLIM 'C' and 'A' thresholds
            // C: EPS Growth >= 25%, Sales Growth >= 20%
            // A: 3-Yr CAGR >= 25%, TTM ROE >= 17%, Unbroken annual progression
            bool passesC = currentQuarter.PassesCriteria;
            bool passesA = annual.PassesCriteria;
            bool passesBoth = passesC && passesA;

            // 4. Calculate IBD SmartSelect-style Composite Fundamental Rating (A+ to E)
            annual.FundamentalGrade = CalculateFundamentalGrade(
                currentQuarter.EpsGrowthYoYPercent,
                currentQuarter.RevenueGrowthYoYPercent,
                annual.EpsCagr3YearPercent,
                annual.ReturnOnEquityPercent,
                currentQuarter.IsAccelerating,
                annual.HasConsecutiveAnnualGrowth);

            return new CanSlimEvaluationResult
            {
                Symbol = cleanSymbol,
                CurrentQuarter = currentQuarter,
                Annual = annual,
                PassesBoth = passesBoth
            };
        }
        private static string CalculateFundamentalGrade(
    decimal qEpsGrowth,
    decimal qRevGrowth,
    decimal annualCagr,
    decimal roe,
    bool isAccelerating,
    bool hasConsecutiveGrowth)
        {
            int score = 0;

            // Quarterly EPS Growth ('C')
            if (qEpsGrowth >= 50m) score += 30;
            else if (qEpsGrowth >= 25m) score += 20;
            else if (qEpsGrowth > 0m) score += 10;

            // Quarterly Sales Confirmation
            if (qRevGrowth >= 25m) score += 15;
            else if (qRevGrowth >= 15m) score += 10;

            // Annual EPS 3-Yr CAGR ('A')
            if (annualCagr >= 35m) score += 25;
            else if (annualCagr >= 25m) score += 15;
            else if (annualCagr > 0m) score += 5;

            // Return on Equity (ROE)
            if (roe >= 25m) score += 20;
            else if (roe >= 17m) score += 15;
            else if (roe >= 10m) score += 5;

            // Acceleration & Consistency Bonuses
            if (isAccelerating) score += 5;
            if (hasConsecutiveGrowth) score += 5;

            // Map 0-100 score to IBD Letter Grades
            return score switch
            {
                >= 90 => "A+",
                >= 80 => "A",
                >= 70 => "B",
                >= 55 => "C",
                >= 40 => "D",
                _ => "E"
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
