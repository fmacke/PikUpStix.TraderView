using System.Net.Http.Json;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Models.Reports;
using TraderView.Domain.Entities.FMP;

namespace TraderView.Services.MarketData
{
    /// <summary>
    /// Service that queries FMP API endpoints concurrently to compile a comprehensive
    /// IBD Stock Checklist / Leaderboard layout for CAN SLIM &amp; SEPA analysis.
    /// </summary>
    public class FinancialModellingPrepCompanyReportService : ICompanyReportService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        private const decimal MinGrowthBaseThreshold = 0.01m;
        private const decimal MaxQuarterlyGrowthCap = 250m;

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
            var normalizedBaseUrl = _baseUrl.TrimEnd('/');
            var todayUtc = DateTime.UtcNow.Date;

            try
            {
                // ----------------------------------------------------------------------------------
                // STAGE 1: Execute concurrent HTTP requests to FMP to minimize latency & API calls
                // ----------------------------------------------------------------------------------
                var profileTask = GetFromJsonAsync<List<FmpCompanyProfile>>(
                    $"{normalizedBaseUrl}/profile?symbol={cleanSymbol}&apikey={_apiKey}");
                var qIncomeTask = GetFromJsonAsync<List<FmpQuarterlyIncomeStatementDto>>(
                    $"{normalizedBaseUrl}/income-statement?symbol={cleanSymbol}&period=quarter&limit=12&apikey={_apiKey}");
                var aIncomeTask = GetFromJsonAsync<List<FmpAnnualIncomeStatementDto>>(
                    $"{normalizedBaseUrl}/income-statement?symbol={cleanSymbol}&period=annual&limit=5&apikey={_apiKey}");
                var metricsTask = GetFromJsonAsync<List<FmpKeyMetricsDto>>(
                    $"{normalizedBaseUrl}/key-metrics-ttm?symbol={cleanSymbol}&limit=1&apikey={_apiKey}");
                var ratiosTask = GetFromJsonAsync<List<FmpRatiosDto>>(
                    $"{normalizedBaseUrl}/ratios-ttm?symbol={cleanSymbol}&limit=1&apikey={_apiKey}");
                var eodTask = GetFromJsonAsync<List<FmpHistoricalBarDto>>(
                    $"{normalizedBaseUrl}/historical-price-eod/full?symbol={cleanSymbol}&timeseries=365&apikey={_apiKey}");
                var calendarTask = GetFromJsonAsync<List<FmpEarningsCalendarDto>>(
                    $"{normalizedBaseUrl}/earnings?symbol={cleanSymbol}&limit=8&apikey={_apiKey}");
                var quarterlyEstimatesTask = GetFromJsonAsync<List<FmpAnalystEstimatesDto>>(
                    $"{normalizedBaseUrl}/analyst-estimates?symbol={cleanSymbol}&period=quarter&limit=4&apikey={_apiKey}");
                var annualEstimatesTask = GetFromJsonAsync<List<FmpAnalystEstimatesDto>>(
                    $"{normalizedBaseUrl}/analyst-estimates?symbol={cleanSymbol}&period=annual&limit=3&apikey={_apiKey}");

                await Task.WhenAll(
                    profileTask, qIncomeTask, aIncomeTask, metricsTask, ratiosTask,
                    eodTask, calendarTask, quarterlyEstimatesTask, annualEstimatesTask);

                var profile = profileTask.Result?.FirstOrDefault();
                var qIncome = qIncomeTask.Result?
                    .OrderByDescending(x => x.Date)
                    .ToList() ?? new List<FmpQuarterlyIncomeStatementDto>();
                var aIncome = aIncomeTask.Result?
                    .OrderByDescending(x => x.Date)
                    .ToList() ?? new List<FmpAnnualIncomeStatementDto>();
                var metrics = metricsTask.Result?.FirstOrDefault();
                var ratios = ratiosTask.Result?.FirstOrDefault();
                var eodData = eodTask.Result ?? new List<FmpHistoricalBarDto>();
                var earningsCalendar = calendarTask.Result ?? new List<FmpEarningsCalendarDto>();
                var quarterlyEstimates = quarterlyEstimatesTask.Result?
                    .OrderBy(e => e.Date)
                    .ToList() ?? new List<FmpAnalystEstimatesDto>();
                var annualEstimates = annualEstimatesTask.Result?
                    .OrderBy(e => e.Date)
                    .ToList() ?? new List<FmpAnalystEstimatesDto>();

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
                var nextEarnings = earningsCalendar
                    .Where(e => !e.EpsActual.HasValue && e.Date >= todayUtc)
                    .OrderBy(e => e.Date)
                    .FirstOrDefault();
                report.EpsDueDate = nextEarnings?.Date;

                var lastReportedEarnings = earningsCalendar
                    .Where(e => e.EpsActual.HasValue && e.EpsEstimated.HasValue && e.Date <= todayUtc)
                    .OrderByDescending(e => e.Date)
                    .FirstOrDefault();

                if (lastReportedEarnings != null &&
                    lastReportedEarnings.EpsEstimated!.Value != 0m &&
                    Math.Abs(lastReportedEarnings.EpsEstimated.Value) >= MinGrowthBaseThreshold)
                {
                    var actual = lastReportedEarnings.EpsActual!.Value;
                    var est = lastReportedEarnings.EpsEstimated.Value;
                    var surprisePct = ((actual - est) / Math.Abs(est)) * 100m;
                    report.LastQtrEarningsSurprisePercent = Math.Round(
                        Math.Clamp(surprisePct, -MaxQuarterlyGrowthCap, MaxQuarterlyGrowthCap), 2);
                }

                if (qIncome.Count >= 5)
                {
                    var q0 = qIncome[0];
                    var q4 = qIncome[4];

                    report.LastQtrEpsGrowthYoY = Math.Round(
                        CalculateGuardedPercentageGrowth(q4.EpsDiluted, q0.EpsDiluted), 2);
                    report.LastQtrSalesGrowthYoY = Math.Round(
                        CalculateGuardedPercentageGrowth(q4.Revenue, q0.Revenue), 2);

                    var growthList = new List<decimal>();
                    int maxQuartersToCompare = Math.Min(3, qIncome.Count - 4);
                    for (int i = 0; i < maxQuartersToCompare; i++)
                    {
                        var curr = qIncome[i].EpsDiluted;
                        var prev = qIncome[i + 4].EpsDiluted;
                        growthList.Add(CalculateGuardedPercentageGrowth(prev, curr));
                    }

                    if (growthList.Count > 0)
                    {
                        report.Last3QtrsAvgEpsGrowth = Math.Round(growthList.Average(), 2);

                        int accelCount = 0;
                        for (int i = 0; i < growthList.Count - 1; i++)
                        {
                            if (growthList[i] > growthList[i + 1])
                                accelCount++;
                            else
                                break;
                        }
                        report.QtrsEpsAccelerationCount = accelCount;
                    }
                }

                PopulateQuarterlyEstimateMetrics(report, quarterlyEstimates, qIncome, todayUtc);
                PopulateAnnualEstimateMetrics(report, annualEstimates, aIncome, todayUtc);

                if (aIncome.Count >= 4)
                {
                    var y0 = aIncome[0].EpsDiluted;
                    var y3 = aIncome[3].EpsDiluted;

                    report.Eps3YrCagr = (y3 > 0m && y0 > 0m)
                        ? Math.Round((decimal)(Math.Pow((double)(y0 / y3), 1.0 / 3.0) - 1.0) * 100m, 2)
                        : 0m;

                    var rev0 = aIncome[0].Revenue;
                    var rev3 = aIncome[3].Revenue;

                    report.Sales3YrCagr = (rev3 > 0m && rev0 > 0m)
                        ? Math.Round((decimal)(Math.Pow((double)(rev0 / rev3), 1.0 / 3.0) - 1.0) * 100m, 2)
                        : 0m;
                }

                if (aIncome.Count >= 2)
                {
                    int consecutiveYrs = 0;
                    for (int i = 0; i < aIncome.Count - 1; i++)
                    {
                        var curr = aIncome[i].EpsDiluted;
                        var prev = aIncome[i + 1].EpsDiluted;

                        if (curr > 0m && curr > prev)
                        {
                            consecutiveYrs++;
                            if (prev <= 0m)
                                break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    report.ConsecutiveYrsAnnualEpsGrowth = consecutiveYrs;
                }

                if (metrics != null)
                {
                    report.AnnualRoePercent = Math.Round(metrics.ReturnOnEquityTTM * 100m, 2);
                }

                if (ratios != null)
                {
                    report.AnnualPreTaxMarginPercent = Math.Round(ratios.PretaxProfitMarginTtm * 100m, 2);
                    report.DebtToEquityRatio = Math.Round(ratios.DebtToEquityRatioTtm, 2);
                }

                report.SmrRating = CalculateSmrGrade(
                    report.LastQtrSalesGrowthYoY,
                    report.AnnualPreTaxMarginPercent,
                    report.AnnualRoePercent);

                report.EpsRating = CalculateEpsRatingProxy(
                    report.LastQtrEpsGrowthYoY,
                    report.Last3QtrsAvgEpsGrowth,
                    report.Eps3YrCagr,
                    report.AnnualRoePercent);

                // ----------------------------------------------------------------------------------
                // STAGE 3: Technical Performance & Volume Analytics (EOD Daily Time Series)
                // ----------------------------------------------------------------------------------
                var sortedEod = eodData
                    .OrderBy(x => x.Date)
                    .ToList();
                var latestBar = sortedEod.Last();

                report.Price = (decimal)latestBar.Close;

                var high52Week = (decimal)sortedEod.Max(x => x.High);
                report.PercentOff52WeekHigh = high52Week > 0
                    ? Math.Round(((report.Price - high52Week) / high52Week) * 100m, 2)
                    : 0m;

                var last50Bars = sortedEod.TakeLast(50).ToList();
                var sma50 = (decimal)last50Bars.Average(x => x.Close);
                report.PriceVs50DaySmaPercent = sma50 > 0
                    ? Math.Round(((report.Price - sma50) / sma50) * 100m, 2)
                    : 0m;
                report.Volume50DayAverage = (long)last50Bars.Average(x => x.Volume);

                decimal upVolumeSum = last50Bars.Where(b => b.Close > b.Open).Sum(b => (decimal)b.Volume);
                decimal downVolumeSum = last50Bars.Where(b => b.Close < b.Open).Sum(b => (decimal)b.Volume);
                report.UpDownVolumeRatio = downVolumeSum > 0
                    ? Math.Round(upVolumeSum / downVolumeSum, 2)
                    : 1.0m;

                report.RelativeStrengthRating = CalculateRsRatingProxy(sortedEod);
                report.AccumulationDistributionRating = CalculateAdRatingProxy(last50Bars);
                report.MarketCapBillions = Math.Round((decimal)profile.MarketCap / 1_000_000_000m, 2);

                report.CompositeRating = CalculateCompositeGrade(
                    report.EpsRating,
                    report.RelativeStrengthRating,
                    report.SmrRating,
                    report.AccumulationDistributionRating);

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

        private static void PopulateQuarterlyEstimateMetrics(
            CompanyChecklistReport report,
            List<FmpAnalystEstimatesDto> orderedQuarterlyEstimates,
            List<FmpQuarterlyIncomeStatementDto> orderedQIncome,
            DateTime todayUtc)
        {
            if (orderedQuarterlyEstimates.Count == 0)
            {
                report.EstimateRevisionsTrend = "Flat";
                return;
            }

            var currentOrNextEst = orderedQuarterlyEstimates.FirstOrDefault(e => e.Date >= todayUtc)
                                   ?? orderedQuarterlyEstimates.Last();

            var priorYearQuarter = orderedQIncome
                .FirstOrDefault(q => Math.Abs((q.Date.AddYears(1) - currentOrNextEst.Date).TotalDays) <= 45);

            if (priorYearQuarter != null &&
                Math.Abs(priorYearQuarter.EpsDiluted) >= MinGrowthBaseThreshold &&
                currentOrNextEst.EpsAvg != 0m)
            {
                var estYoY = ((currentOrNextEst.EpsAvg - priorYearQuarter.EpsDiluted) /
                              Math.Abs(priorYearQuarter.EpsDiluted)) * 100m;
                report.CurrentQtrEpsEstChg = Math.Round(
                    Math.Clamp(estYoY, -MaxQuarterlyGrowthCap, MaxQuarterlyGrowthCap), 2);
            }

            if (orderedQuarterlyEstimates.Count >= 2)
            {
                var first = orderedQuarterlyEstimates[0].EpsAvg;
                var last = orderedQuarterlyEstimates[^1].EpsAvg;
                if (last > first * 1.02m)
                    report.EstimateRevisionsTrend = "Up";
                else if (last < first * 0.98m)
                    report.EstimateRevisionsTrend = "Down";
                else
                    report.EstimateRevisionsTrend = "Flat";
            }
            else
            {
                report.EstimateRevisionsTrend = "Flat";
            }
        }

        private static void PopulateAnnualEstimateMetrics(
            CompanyChecklistReport report,
            List<FmpAnalystEstimatesDto> orderedAnnualEstimates,
            List<FmpAnnualIncomeStatementDto> orderedAIncome,
            DateTime todayUtc)
        {
            if (orderedAnnualEstimates.Count == 0 || orderedAIncome.Count == 0)
                return;

            var lastReportedAnnual = orderedAIncome[0];
            var hasLastAnnualDate = DateTime.TryParse(lastReportedAnnual.Date, out var lastAnnualDate);

            var currentYearEst = orderedAnnualEstimates
                .Where(e => (!hasLastAnnualDate || e.Date > lastAnnualDate.AddDays(45)) && e.Date >= todayUtc.AddDays(-90))
                .OrderBy(e => e.Date)
                .FirstOrDefault()
                ?? orderedAnnualEstimates.FirstOrDefault(e => e.Date >= todayUtc);

            if (currentYearEst != null &&
                Math.Abs(lastReportedAnnual.EpsDiluted) >= MinGrowthBaseThreshold &&
                currentYearEst.EpsAvg != 0m)
            {
                var estYoY = ((currentYearEst.EpsAvg - lastReportedAnnual.EpsDiluted) /
                              Math.Abs(lastReportedAnnual.EpsDiluted)) * 100m;
                report.CurrentYearEpsEstChg = Math.Round(estYoY, 2);
            }
        }

        private static decimal CalculateGuardedPercentageGrowth(decimal baseValue, decimal currentValue)
        {
            if (Math.Abs(baseValue) < MinGrowthBaseThreshold)
                return 0m;

            var rawPct = ((currentValue - baseValue) / Math.Abs(baseValue)) * 100m;
            return Math.Clamp(rawPct, -MaxQuarterlyGrowthCap, MaxQuarterlyGrowthCap);
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
            decimal cappedQ = Math.Clamp(qEpsGrowth, -50m, 75m);
            decimal cappedAvg = Math.Clamp(q3AvgGrowth, -50m, 75m);
            decimal cappedCagr = Math.Clamp(cagr3Yr, -25m, 75m);
            decimal cappedRoe = Math.Clamp(roe, -25m, 50m);

            decimal rawScore = 35m
                               + (cappedQ * 0.20m)
                               + (cappedAvg * 0.15m)
                               + (cappedCagr * 0.15m)
                               + (cappedRoe * 0.10m);

            return Math.Clamp((int)Math.Round(rawScore), 1, 99);
        }

        private static int CalculateRsRatingProxy(List<FmpHistoricalBarDto> sortedEod)
        {
            if (sortedEod.Count < 250) return 50;

            var currentClose = (decimal)sortedEod.Last().Close;
            var q1Close = (decimal)sortedEod[sortedEod.Count - 63].Close;
            var q2Close = (decimal)sortedEod[sortedEod.Count - 126].Close;
            var q3Close = (decimal)sortedEod[sortedEod.Count - 189].Close;
            var q4Close = (decimal)sortedEod[0].Close;

            decimal r1 = q1Close > 0 ? ((currentClose - q1Close) / q1Close) * 100m : 0m;
            decimal r2 = q2Close > 0 ? ((currentClose - q2Close) / q2Close) * 100m : 0m;
            decimal r3 = q3Close > 0 ? ((currentClose - q3Close) / q3Close) * 100m : 0m;
            decimal r4 = q4Close > 0 ? ((currentClose - q4Close) / q4Close) * 100m : 0m;

            decimal weightedPerformance = (r1 * 0.40m) + (r2 * 0.20m) + (r3 * 0.20m) + (r4 * 0.20m);
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
            return Math.Clamp(composite, 1, 99).ToString();
        }

        #endregion
    }
}