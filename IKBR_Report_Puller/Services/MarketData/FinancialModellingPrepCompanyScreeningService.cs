using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities.FMP;

namespace PikUpStix.TraderView.Services.MarketData
{
    /// <summary>
    /// Service for running company screening and retrieving company financial data for analysis
    /// </summary>
    public class FinancialModellingPrepCompanyScreeningService : ICompanyScreeningService
    {
        private readonly HttpClient _httpClient;
        private readonly ICanSlimScreenerService _canSlimScreenerService;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public FinancialModellingPrepCompanyScreeningService(
            HttpClient httpClient,
            ICanSlimScreenerService canSlimScreenerService,
            string apiKey,
            string baseUrl)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _canSlimScreenerService = canSlimScreenerService ?? throw new ArgumentNullException(nameof(canSlimScreenerService));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        async Task<IReadOnlyList<CanSlimCandidate>> ICompanyScreeningService.RunScreenerAsync(CanSlimScreenerCriteria criteria)
        {
            var latestScreener = await _canSlimScreenerService.GetLatestScreenerSnapShot();
            if (latestScreener == null || latestScreener.CreatedAt < DateTime.Today)
            {
                var newScreenerData = await GetNewScreenerData(criteria);
                await _canSlimScreenerService.CreateCanSlimScreenerSnapshot(newScreenerData.ToList());
                return newScreenerData;
            }
            else
                return await _canSlimScreenerService.GetAllBySnapshotIdAsync(latestScreener.Id);
        }

        async Task<IReadOnlyList<CanSlimCandidate>> ICompanyScreeningService.GetLatestScreenerResults()
        {
            var latestScreener = await _canSlimScreenerService.GetLatestScreenerSnapShot();
            if (latestScreener == null)
            {
                return Array.Empty<CanSlimCandidate>();
            }
            else
            {
                return await _canSlimScreenerService.GetAllBySnapshotIdAsync(latestScreener.Id);
            }
        }

        async Task<IReadOnlyList<FmpQuarterlyIncomeStatementDto>> ICompanyScreeningService.GetQuarterlyIncomeStatementsAsync(string symbol, int limit = 8)
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

        async Task<IReadOnlyList<FmpAnnualIncomeStatementDto>> ICompanyScreeningService.GetAnnualIncomeStatementsAsync(string symbol, int limit = 5)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                throw new ArgumentException("Ticker symbol cannot be null or whitespace.", nameof(symbol));
            }

            var cleanSymbol = symbol.Trim().ToUpperInvariant();

            try
            {
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

        async Task<IReadOnlyList<FmpKeyMetricsDto>> ICompanyScreeningService.GetKeyMetricsTtmAsync(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                throw new ArgumentException("Ticker symbol cannot be null or whitespace.", nameof(symbol));
            }

            var cleanSymbol = symbol.Trim().ToUpperInvariant();

            try
            {
                var url = $"{_baseUrl}/key-metrics-ttm?symbol={cleanSymbol}&period=annual&limit=1&apikey={_apiKey}";

                var result = await _httpClient.GetFromJsonAsync<List<FmpKeyMetricsDto>>(url);

                return result ?? (IReadOnlyList<FmpKeyMetricsDto>)Array.Empty<FmpKeyMetricsDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP error fetching TTM key metrics for {cleanSymbol} from FMP (Status: {ex.StatusCode}): {ex.Message}");
                return Array.Empty<FmpKeyMetricsDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error fetching TTM key metrics for {cleanSymbol}: {ex.Message}");
                return Array.Empty<FmpKeyMetricsDto>();
            }
        }

        async Task<CanSlimCurrentQuarterMetric?> ICompanyScreeningService.EvaluateCurrentQuarterEpsAsync(
            string symbol,
            decimal minEpsGrowth = 25m,
            decimal minRevenueGrowth = 20m)
        {
            // Fetch at least 8 quarters to evaluate YoY growth across consecutive recent quarters
            var statements = await ((ICompanyScreeningService)this).GetQuarterlyIncomeStatementsAsync(symbol, 8);

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

        async Task<CanSlimAnnualMetric?> ICompanyScreeningService.EvaluateAnnualEpsAsync(string symbol, decimal minCagr = 25m, decimal minRoe = 17m)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                throw new ArgumentException("Ticker symbol cannot be null or whitespace.", nameof(symbol));
            }

            var cleanSymbol = symbol.Trim().ToUpperInvariant();

            // 1. Concurrently fetch 5 years of annual income statements and TTM key metrics
            var annualsTask = ((ICompanyScreeningService)this).GetAnnualIncomeStatementsAsync(cleanSymbol, limit: 5);
            var metricsTask = ((ICompanyScreeningService)this).GetKeyMetricsTtmAsync(cleanSymbol);

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
                operatingMargin = Math.Round(primaryMetric.NetProfitMargin * 100m, 2);
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
            // O'Neil's requirement: 3-Yr CAGR >= 25% AND TTM ROE >= 17% AND unbroken 3-yr annual progression
            bool passesA = cagr3YearPercent >= minCagr && returnOnEquity >= minRoe;

            return new CanSlimAnnualMetric
            {
                Symbol = cleanSymbol,
                LatestFiscalYear = annuals[0].CalendarYear,
                LatestFiscalYearEps = y0,
                PriorYear1Eps = y1,
                PriorYear2Eps = y2,
                PriorYear3Eps = y3,
                EpsCagr3YearPercent = cagr3YearPercent,
                EpsCagr5YearPercent = cagr5YearPercent,
                ReturnOnEquityPercent = returnOnEquity,
                OperatingMarginPercent = operatingMargin,
                ReturnOnAssetsPercent = returnOnAssets,
                HasConsecutiveAnnualGrowth = hasConsecutiveGrowth,
                AnnualHistory = history,
                PassesCriteria = passesA
            };
        }

        private async Task<IReadOnlyList<CanSlimCandidate>> GetNewScreenerData(CanSlimScreenerCriteria criteria)
        {
            // CALL FMP API to get new candidates and save to database
            // STAGE 1: Bulk screener API call to fetch liquid universe
            var url = $"{_baseUrl}/company-screener?priceMoreThan={criteria.MinPrice}&volumeMoreThan={criteria.MinVolume}&marketCapMoreThan={criteria.MinMarketCap}&isEtf=false&isActivelyTrading=true&exchange=NASDAQ,NYSE&country=US&limit={criteria.Stage1UniverseLimit}&apikey={_apiKey}";

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

                    if (StockPassesEvaluation(caResult, criteria))
                    {
                        passedCandidates.Add(new CanSlimCandidate
                        {
                            Symbol = stock.Symbol,
                            Exchange = stock.Exchange,
                            CompanyName = stock.CompanyName,
                            Sector = stock.Sector,
                            Industry = stock.Industry,
                            Price = stock.Price,
                            Volume = stock.Volume,
                            MarketCap = stock.MarketCap,
                            CurrentQuarterLatestQuarterDate = caResult.CurrentQuarter?.LatestQuarterDate,
                            CurrentQuarterLatestQuarterEps = caResult.CurrentQuarter?.LatestQuarterEps ?? 0m,
                            CurrentQuarterPriorYearQuarterEps = caResult.CurrentQuarter?.PriorYearQuarterEps ?? 0m,
                            CurrentQuarterEpsGrowthYoYpercent = caResult.CurrentQuarter?.EpsGrowthYoYPercent ?? 0m,
                            CurrentQuarterRevenueGrowthYoYpercent = caResult.CurrentQuarter?.RevenueGrowthYoYPercent ?? 0m,
                            CurrentQuarterIsAccelerating = caResult.CurrentQuarter?.IsAccelerating ?? false,
                            CurrentQuarterPassesCriteria = caResult.CurrentQuarter?.PassesCriteria ?? false,
                            AnnualEpsCagr3YearPercent = caResult.Annual?.EpsCagr3YearPercent ?? 0m,
                            AnnualEpsCagr5YearPercent = caResult.Annual?.EpsCagr5YearPercent,
                            AnnualReturnOnEquityPercent = caResult.Annual?.ReturnOnEquityPercent ?? 0m,
                            AnnualHasConsecutiveAnnualGrowth = caResult.Annual?.HasConsecutiveAnnualGrowth ?? false,
                            AnnualLatestFiscalYear = caResult.Annual?.LatestFiscalYear,
                            AnnualLatestFiscalYearEps = caResult.Annual?.LatestFiscalYearEps ?? 0m,
                            AnnualPriorYear1Eps = caResult.Annual?.PriorYear1Eps ?? 0m,
                            AnnualPriorYear2Eps = caResult.Annual?.PriorYear2Eps ?? 0m,
                            AnnualPriorYear3Eps = caResult.Annual?.PriorYear3Eps ?? 0m,
                            AnnualOperatingMarginPercent = caResult.Annual?.OperatingMarginPercent ?? 0m,
                            AnnualReturnOnAssetsPercent = caResult.Annual?.ReturnOnAssetsPercent ?? 0m,
                            AnnualFundamentalGrade = caResult.Annual?.FundamentalGrade,
                            AnnualPassesCriteria = caResult.Annual?.PassesCriteria ?? false,
                            EvaluationDateUtc = DateTime.UtcNow,
                            CreatedAtUtc = DateTime.UtcNow,
                            PassesBoth = Convert.ToBoolean(caResult.CurrentQuarter?.PassesCriteria) && Convert.ToBoolean(caResult.Annual?.PassesCriteria) ? true : false
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
                .OrderByDescending(x => x.CurrentQuarterEpsGrowthYoYpercent)
                .ToList();
        }

        private bool StockPassesEvaluation(CanSlimEvaluationResult caResult, CanSlimScreenerCriteria criteria)
        {
            return caResult != null &&
                   caResult.CurrentQuarter != null &&
                   caResult.Annual != null &&
                   caResult.CurrentQuarter.EpsGrowthYoYPercent >= criteria.MinCurrentQuarterEpsGrowthPercent &&
                   caResult.CurrentQuarter.RevenueGrowthYoYPercent >= criteria.MinCurrentQuarterRevGrowthPercent &&
                   caResult.Annual.EpsCagr3YearPercent >= criteria.MinAnnualEpsCagrPercent &&
                   caResult.Annual.ReturnOnEquityPercent >= criteria.MinReturnOnEquityPercent;
        }

        private async Task<CanSlimEvaluationResult> EvaluateCanSlimCAAsync(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                throw new ArgumentException("Ticker symbol cannot be null or whitespace.", nameof(symbol));
            }

            var cleanSymbol = symbol.Trim().ToUpperInvariant();

            // 1. Run 'C' (Quarterly) and 'A' (Annual) evaluations concurrently to minimize API latency
            var currentQuarterTask = ((ICompanyScreeningService)this).EvaluateCurrentQuarterEpsAsync(cleanSymbol);
            var annualTask = ((ICompanyScreeningService)this).EvaluateAnnualEpsAsync(cleanSymbol);

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

        private static string CalculateFundamentalGrade(decimal qEpsGrowth, decimal qRevGrowth, decimal annualCagr, decimal roe, bool isAccelerating, bool hasConsecutiveGrowth)
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
    }
}
