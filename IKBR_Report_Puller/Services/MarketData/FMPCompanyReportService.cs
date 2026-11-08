using TraderView.Application.Models.Reports;
using System.Net.Http.Json;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities.FMP;

namespace TraderView.Services.MarketData
{
    /// <summary>
    /// Service that queries FMP API endpoints concurrently to compile a comprehensive 
    /// IBD Stock Checklist / Leaderboard layout for CAN SLIM & SEPA analysis.
    /// </summary>
    public class FinancialModellingPrepCompanyReportService : ICompanyReportService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public FinancialModellingPrepCompanyReportService(
            HttpClient httpClient,
            string apiKey,
            string baseUrl)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        public async Task<CompanyChecklistReport?> GenerateChecklistReportAsync(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentException("Ticker symbol cannot be null or whitespace.", nameof(symbol));

            var cleanSymbol = symbol.Trim().ToUpperInvariant();

            try
            {
                // ----------------------------------------------------------------------------------
                // STAGE 1: Execute concurrent HTTP requests to FMP to minimize latency & API calls
                // ----------------------------------------------------------------------------------
                var profileTask = GetFromJsonAsync<List<FmpCompanyProfile>>($"{_baseUrl}/profile?symbol={cleanSymbol}&apikey={_apiKey}");
                var qIncomeTask = GetFromJsonAsync<List<FmpQuarterlyIncomeStatementDto>>($"{_baseUrl}/income-statement?symbol={cleanSymbol}&period=quarter&limit=12&apikey={_apiKey}");
                var aIncomeTask = GetFromJsonAsync<List<FmpAnnualIncomeStatementDto>>($"{_baseUrl}/income-statement?symbol={cleanSymbol}&period=annual&limit=5&apikey={_apiKey}");
                var metricsTask = GetFromJsonAsync<List<FmpKeyMetricsDto>>($"{_baseUrl}/key-metrics-ttm?symbol={cleanSymbol}&limit=1&apikey={_apiKey}");
                var ratiosTask = GetFromJsonAsync<List<FmpRatiosDto>>($"{_baseUrl}/ratios-ttm?symbol={cleanSymbol}&limit=1&apikey={_apiKey}");
                var eodTask = GetFromJsonAsync<List<FmpHistoricalBarDto>>($"{_baseUrl}/historical-price-eod/full?symbol={cleanSymbol}&timeseries=365&apikey={_apiKey}");
                var calendarTask = GetFromJsonAsync<List<FmpEarningsCalendarDto>>($"{_baseUrl}/earnings?symbol={cleanSymbol}&limit=4&apikey={_apiKey}");
                var estimatesTask = GetFromJsonAsync<List<FmpAnalystEstimatesDto>>($"{_baseUrl}/analyst-estimates?symbol={cleanSymbol}&period=quarter&limit=4&apikey={_apiKey}");
                var surprisesTask = GetFromJsonAsync<List<FmpEarningsSurpriseDto>>($"{_baseUrl}/earnings-surprises?symbol={cleanSymbol}&limit=4&apikey={_apiKey}");
                var fundHoldersTask = GetFromJsonAsync<List<FmpInstitutionalHolderDto>>($"{_baseUrl}/institutional-holder?symbol={cleanSymbol}&apikey={_apiKey}");

                await Task.WhenAll(
                    profileTask, qIncomeTask, aIncomeTask, metricsTask, ratiosTask,
                    eodTask, calendarTask, estimatesTask, surprisesTask, fundHoldersTask);

                var profile = profileTask.Result?.FirstOrDefault();
                var qIncome = qIncomeTask.Result ?? new List<FmpQuarterlyIncomeStatementDto>();
                var aIncome = aIncomeTask.Result?.OrderByDescending(x => x.CalendarYear).ToList() ?? new List<FmpAnnualIncomeStatementDto>();
                var metrics = metricsTask.Result?.FirstOrDefault();
                var ratios = ratiosTask.Result?.FirstOrDefault();
                var eodData = eodTask.Result ?? new List<FmpHistoricalBarDto>();
                var earningsCalendar = calendarTask.Result;
                var estimates = estimatesTask.Result;
                var surprises = surprisesTask.Result;
                var fundHolders = fundHoldersTask.Result;

                if (profile == null || qIncome.Count < 5 || eodData.Count < 50)
                {
                    Console.WriteLine($"Insufficient core dataset available to construct report for {cleanSymbol}");
                    return null;
                }

                var report = new CompanyChecklistReport
                {
                    Symbol = cleanSymbol,
                    CompanyName = profile.CompanyName ?? cleanSymbol,
                    IndustryGroup = profile.Industry ?? "N/A"
                };

                // ----------------------------------------------------------------------------------
                // STAGE 2: Fundamental Metrics Parsing (Current & Annual Earnings)
                // ----------------------------------------------------------------------------------
                // EPS Due Date
                var nextEarnings = earningsCalendar?.FirstOrDefault(e => e.Date >= DateTime.UtcNow.Date);
                report.EpsDueDate = nextEarnings?.Date;

                // Quarterly EPS Growth Rates (Latest YoY, 3-Qtr Avg, and Acceleration Count)
                if (qIncome.Count >= 5)
                {
                    var q0 = qIncome[0];
                    var q4 = qIncome[4];
                    report.LastQtrEpsGrowthYoY = CalculatePercentageGrowth(q4.EpsDiluted, q0.EpsDiluted);
                    report.LastQtrSalesGrowthYoY = CalculatePercentageGrowth(q4.Revenue, q0.Revenue);

                    // Compute 3-Quarter Average EPS Growth
                    var growthList = new List<decimal>();
                    for (int i = 0; i < Math.Min(3, qIncome.Count - 4); i++)
                    {
                        var curr = qIncome[i].EpsDiluted;
                        var prev = qIncome[i + 4].EpsDiluted;
                        growthList.Add(CalculatePercentageGrowth(prev, curr));
                    }
                    report.Last3QtrsAvgEpsGrowth = Math.Round(growthList.Average(), 2);

                    // Count consecutive quarters of acceleration
                    int accelCount = 0;
                    for (int i = 0; i < growthList.Count - 1; i++)
                    {
                        if (growthList[i] > growthList[i + 1]) accelCount++;
                        else break;
                    }
                    report.QtrsEpsAccelerationCount = accelCount;
                }

                // Earnings Surprise
                if (surprises != null && surprises.Count > 0)
                {
                    report.LastQtrEarningsSurprisePercent = Math.Round(surprises[0].SurprisePercent, 2);
                }

                // Annual EPS CAGR & Consecutive Years of Growth
                if (aIncome.Count >= 4)
                {
                    var y0 = aIncome[0].EpsDiluted;
                    var y3 = aIncome[3].EpsDiluted;

                    if (y3 > 0 && y0 > 0)
                    {
                        double ratio = (double)(y0 / y3);
                        report.Eps3YrCagr = Math.Round((decimal)(Math.Pow(ratio, 1.0 / 3.0) - 1.0) * 100m, 2);
                    }

                    int consecutiveYrs = 0;
                    for (int i = 0; i < aIncome.Count - 1; i++)
                    {
                        if (aIncome[i].EpsDiluted > aIncome[i + 1].EpsDiluted) consecutiveYrs++;
                        else break;
                    }
                    report.ConsecutiveYrsAnnualEpsGrowth = consecutiveYrs;

                    // 3-Year Sales CAGR
                    var rev0 = aIncome[0].Revenue;
                    var rev3 = aIncome[3].Revenue;
                    if (rev3 > 0 && rev0 > 0)
                    {
                        double revRatio = (double)(rev0 / rev3);
                        report.Sales3YrCagr = Math.Round((decimal)(Math.Pow(revRatio, 1.0 / 3.0) - 1.0) * 100m, 2);
                    }
                }

                // Margins, ROE & Debt/Equity from TTM Ratios/Metrics
                if (metrics != null)
                {
                    report.AnnualRoePercent = Math.Round(metrics.Roe * 100m, 2);
                }
                if (ratios != null)
                {
                    report.AnnualPreTaxMarginPercent = Math.Round(ratios.PretaxProfitMargin * 100m, 2);
                    report.DebtToEquityRatio = Math.Round(ratios.DebtEquityRatio, 2);
                }

                // Compute SMR Grade (Sales, Margin, ROE)
                report.SmrRating = CalculateSmrGrade(report.LastQtrSalesGrowthYoY, report.AnnualPreTaxMarginPercent, report.AnnualRoePercent);

                // Compute Overall EPS Rating Grade (1-99 Proxy)
                report.EpsRating = CalculateEpsRatingProxy(report.LastQtrEpsGrowthYoY, report.Last3QtrsAvgEpsGrowth, report.Eps3YrCagr, report.AnnualRoePercent);

                // ----------------------------------------------------------------------------------
                // STAGE 3: Technical Performance & Volume Analytics (EOD Daily Time Series)
                // ----------------------------------------------------------------------------------
                var sortedEod = eodData.OrderBy(x => x.Date).ToList();
                var latestBar = sortedEod.Last();

                report.Price = (decimal)latestBar.Close;

                // 52-Week High & Spread
                var high52Week = (decimal)sortedEod.Max(x => x.High);
                report.PercentOff52WeekHigh = Math.Round(((report.Price - high52Week) / high52Week) * 100m, 2);

                // 50-Day Simple Moving Average & Spread
                var last50Bars = sortedEod.TakeLast(50).ToList();
                var sma50 = (decimal)last50Bars.Average(x => x.Close);
                report.PriceVs50DaySmaPercent = Math.Round(((report.Price - sma50) / sma50) * 100m, 2);
                report.Volume50DayAverage = (long)last50Bars.Average(x => x.Volume);

                // Up/Down Volume Ratio (50-Day)
                decimal upVolumeSum = last50Bars.Where(b => b.Close > b.Open).Sum(b => (decimal)b.Volume);
                decimal downVolumeSum = last50Bars.Where(b => b.Close < b.Open).Sum(b => (decimal)b.Volume);
                report.UpDownVolumeRatio = downVolumeSum > 0 ? Math.Round(upVolumeSum / downVolumeSum, 2) : 1.0m;

                // Relative Strength (RS) Rating Proxy (Weighted 12-Month Price Performance)
                report.RelativeStrengthRating = CalculateRsRatingProxy(sortedEod);

                // Accumulation/Distribution (A/D) Grade Proxy
                report.AccumulationDistributionRating = CalculateAdRatingProxy(last50Bars);

                // Supply & Demand - Market Cap
                report.MarketCapBillions = Math.Round((decimal)profile.MarketCap / 1_000_000_000m, 2);

                // Fundamental Composite Score
                report.CompositeRating = CalculateCompositeGrade(report.EpsRating, report.RelativeStrengthRating, report.SmrRating, report.AccumulationDistributionRating);

                return report;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error compiling company report for {cleanSymbol}: {ex.Message}");
                return null;
            }
        }

        #region Helper Methods & Calculation Logic

        private async Task<T?> GetFromJsonAsync<T>(string url)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<T>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTTP Fetch error on URL [{url}]: {ex.Message}");
                return default;
            }
        }

        private static decimal CalculatePercentageGrowth(decimal baseValue, decimal currentValue)
        {
            if (baseValue == 0) return currentValue > 0 ? 100m : 0m;
            return ((currentValue - baseValue) / Math.Abs(baseValue)) * 100m;
        }

        private static string CalculateSmrGrade(decimal salesGrowth, decimal preTaxMargin, decimal roe)
        {
            int score = 0;
            if (salesGrowth >= 25m) score += 35; else if (salesGrowth >= 15m) score += 20;
            if (preTaxMargin >= 18m) score += 35; else if (preTaxMargin >= 10m) score += 20;
            if (roe >= 25m) score += 30; else if (roe >= 17m) score += 20;

            return score switch
            {
                >= 85 => "A",
                >= 70 => "B",
                >= 55 => "C",
                >= 40 => "D",
                _ => "E"
            };
        }

        private static int CalculateEpsRatingProxy(decimal qEpsGrowth, decimal q3AvgGrowth, decimal cagr3Yr, decimal roe)
        {
            decimal rawScore = (qEpsGrowth * 0.35m) + (q3AvgGrowth * 0.25m) + (cagr3Yr * 0.25m) + (roe * 0.15m);
            // Clamp score between 1 and 99 percentile range
            return Math.Clamp((int)Math.Round(rawScore), 1, 99);
        }

        private static int CalculateRsRatingProxy(List<FmpHistoricalBarDto> sortedEod)
        {
            if (sortedEod.Count < 250) return 50;

            // Compute 3-month, 6-month, 9-month, and 12-month returns
            var currentClose = (decimal)sortedEod.Last().Close;
            var q1Close = (decimal)sortedEod[sortedEod.Count - 63].Close;
            var q2Close = (decimal)sortedEod[sortedEod.Count - 126].Close;
            var q3Close = (decimal)sortedEod[sortedEod.Count - 189].Close;
            var q4Close = (decimal)sortedEod[0].Close;

            decimal r1 = ((currentClose - q1Close) / q1Close) * 100m;
            decimal r2 = ((currentClose - q2Close) / q2Close) * 100m;
            decimal r3 = ((currentClose - q3Close) / q3Close) * 100m;
            decimal r4 = ((currentClose - q4Close) / q4Close) * 100m;

            // O'Neil RS weighting: 40% Q1, 20% Q2, 20% Q3, 20% Q4
            decimal weightedPerformance = (r1 * 0.40m) + (r2 * 0.20m) + (r3 * 0.20m) + (r4 * 0.20m);

            // Transform raw performance into a normalized 1-99 score
            return Math.Clamp((int)(50 + (weightedPerformance * 0.8m)), 1, 99);
        }

        private static string CalculateAdRatingProxy(List<FmpHistoricalBarDto> last50Bars)
        {
            decimal avgVolume = (decimal)last50Bars.Average(b => b.Volume);
            int score = 50;

            foreach (var bar in last50Bars)
            {
                bool isUpDay = bar.Close > bar.Open;
                bool isAboveAvgVol = (decimal)bar.Volume > avgVolume;

                if (isUpDay && isAboveAvgVol) score += 3;
                else if (!isUpDay && isAboveAvgVol) score -= 3;
            }

            return score switch
            {
                >= 70 => "A",
                >= 60 => "B",
                >= 45 => "C",
                >= 30 => "D",
                _ => "E"
            };
        }

        private static string CalculateCompositeGrade(int epsRating, int rsRating, string smrRating, string adRating)
        {
            int smrNumeric = smrRating switch { "A" => 95, "B" => 80, "C" => 65, "D" => 50, _ => 30 };
            int adNumeric = adRating switch { "A" => 95, "B" => 80, "C" => 65, "D" => 50, _ => 30 };

            int composite = (int)((epsRating * 0.30m) + (rsRating * 0.30m) + (smrNumeric * 0.20m) + (adNumeric * 0.20m));
            return composite.ToString();
        }

        #endregion
    }
}