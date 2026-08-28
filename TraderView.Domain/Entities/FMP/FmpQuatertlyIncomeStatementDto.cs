using System.Text.Json.Serialization;

namespace TraderView.Domain.Entities.FMP
{
    public class FmpQuarterlyIncomeStatementDto
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("period")]
        public string Period { get; set; } = string.Empty;

        [JsonPropertyName("calendarYear")]
        public string CalendarYear { get; set; } = string.Empty;

        [JsonPropertyName("revenue")]
        public decimal Revenue { get; set; }

        [JsonPropertyName("eps")]
        public decimal Eps { get; set; }

        [JsonPropertyName("epsdiluted")]
        public decimal EpsDiluted { get; set; }

        [JsonPropertyName("fillingDate")]
        public string FillingDate { get; set; } = string.Empty;
    }
    /// <summary>
    /// Represents the evaluation of William O'Neil's CAN SLIM 'A' (Annual Earnings) pillar,
    /// tracking multi-year EPS growth, ROE leader thresholds, and margin sustainability.
    /// </summary>
    public class CanSlimAnnualMetric
    {
        public string Symbol { get; set; } = string.Empty;
        public DateTime EvaluationDateUtc { get; set; } = DateTime.UtcNow;

        #region Core O'Neil Criteria (EPS & ROE)

        /// <summary>
        /// 3-Year Compound Annual Growth Rate (CAGR) of Diluted EPS.
        /// O'Neil Benchmark: >= 25% minimum, ideally >= 50% for top leaders.
        /// </summary>
        public decimal EpsCagr3YearPercent { get; set; }

        /// <summary>
        /// 5-Year Compound Annual Growth Rate (CAGR) of Diluted EPS (if available).
        /// </summary>
        public decimal? EpsCagr5YearPercent { get; set; }

        /// <summary>
        /// Trailing Twelve Months (TTM) Return on Equity (ROE).
        /// O'Neil Benchmark: >= 17% minimum, top leaders typically 25% - 50%+.
        /// </summary>
        public decimal ReturnOnEquityPercent { get; set; }

        /// <summary>
        /// Indicates if EPS increased year-over-year in each of the last 3 fiscal years (no down years).
        /// </summary>
        public bool HasConsecutiveAnnualGrowth { get; set; }

        #endregion

        #region Historical Annual EPS Breakdown

        /// <summary>
        /// Most recently completed fiscal year diluted EPS (Y0).
        /// </summary>
        public decimal LatestFiscalYearEps { get; set; }

        /// <summary>
        /// Fiscal year calendar label for Y0 (e.g., "2025").
        /// </summary>
        public string LatestFiscalYear { get; set; } = string.Empty;

        /// <summary>
        /// Prior fiscal year diluted EPS (Y-1).
        /// </summary>
        public decimal PriorYear1Eps { get; set; }

        /// <summary>
        /// 2 years prior fiscal year diluted EPS (Y-2).
        /// </summary>
        public decimal PriorYear2Eps { get; set; }

        /// <summary>
        /// 3 years prior fiscal year diluted EPS (Y-3).
        /// </summary>
        public decimal PriorYear3Eps { get; set; }

        /// <summary>
        /// Full annual EPS progression array (latest first) for historical plotting / breakdown.
        /// </summary>
        public IReadOnlyList<AnnualEarningsPoint> AnnualHistory { get; set; } = Array.Empty<AnnualEarningsPoint>();

        #endregion

        #region Supporting Fundamentals (Quality of Earnings)

        /// <summary>
        /// Pre-tax profit margin or operating margin TTM (%).
        /// Quality check: Leaders demonstrate expanding or top-tier margins in their industry group.
        /// </summary>
        public decimal OperatingMarginPercent { get; set; }

        /// <summary>
        /// Return on Assets (ROA) TTM (%) as a secondary efficiency indicator.
        /// </summary>
        public decimal ReturnOnAssetsPercent { get; set; }

        /// <summary>
        /// Indicates if the stock meets the minimum CAN SLIM 'A' standards:
        /// 3-Yr CAGR >= 25%, ROE >= 17%, and uninterrupted annual EPS growth.
        /// </summary>
        public bool PassesCriteria { get; set; }

        /// <summary>
        /// Letter grade rating (A+, A, B, C, D, E) emulating IBD SmartSelect / Leaderboard fundamental ratings.
        /// </summary>
        public string FundamentalGrade { get; set; } = "N/A";

        #endregion
    }

    /// <summary>
    /// Represents a single fiscal year's reported financial metrics.
    /// </summary>
    public class AnnualEarningsPoint
    {
        public int CandidateId { get; set; }
        public string CalendarYear { get; set; } = string.Empty;
        public string FiscalDate { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal NetIncome { get; set; }
        public decimal EpsDiluted { get; set; }
        public decimal EpsGrowthYoYPercent { get; set; }
    }
    public class CanSlimEvaluationResult
    {
        public string Symbol { get; set; } = string.Empty;
        public CanSlimCurrentQuarterMetric? CurrentQuarter { get; set; }
        public CanSlimAnnualMetric? Annual { get; set; }
        public bool PassesBoth { get; set; }
    }
    public class FmpAnnualIncomeStatementDto
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("fiscalYear")]
        public string CalendarYear { get; set; } = string.Empty;

        [JsonPropertyName("period")]
        public string Period { get; set; } = string.Empty; // "FY"

        [JsonPropertyName("revenue")]
        public decimal Revenue { get; set; }

        [JsonPropertyName("grossProfit")]
        public decimal GrossProfit { get; set; }

        [JsonPropertyName("operatingIncome")]
        public decimal OperatingIncome { get; set; }

        [JsonPropertyName("netIncome")]
        public decimal NetIncome { get; set; }

        [JsonPropertyName("eps")]
        public decimal Eps { get; set; }

        [JsonPropertyName("epsdiluted")]
        public decimal EpsDiluted { get; set; }

        [JsonPropertyName("fillingDate")]
        public string FillingDate { get; set; } = string.Empty;
    }

    public class FmpKeyMetricsDto
    {
        [JsonPropertyName("returnOnEquityTTM")]
        public decimal Roe { get; set; } // e.g., 0.3245 -> 32.45% (O'Neil Benchmark >= 17%)

        [JsonPropertyName("returnOnTangibleAssetsTTM")]
        public decimal ReturnOnTangibleAssets { get; set; }

        [JsonPropertyName("incomeQualityTTM")]
        public decimal IncomeQualityTTM { get; set; }

        [JsonPropertyName("operatingReturnOnAssetsTTM")]
        public decimal OperatingReturnOnAssetsTTM { get; set; }

        [JsonPropertyName("marketCap")]
        public decimal MarketCap { get; set; }

        [JsonPropertyName("enterpriseValueTTM")]
        public decimal EnterpriseValueTTM { get; set; }

        [JsonPropertyName("evToSalesTTM")]
        public decimal EvToSalesTTM { get; set; }

        [JsonPropertyName("earningsYieldTTM")]
        public decimal EarningsYieldTTM { get; set; }

        // Note: Net profit margin is not present in this metrics payload (would typically be in financial ratios)
        public decimal NetProfitMargin =>
        EnterpriseValueTTM > 0 && EvToSalesTTM > 0
            ? EarningsYieldTTM * (MarketCap / EnterpriseValueTTM) * EvToSalesTTM
            : 0m;

        [JsonPropertyName("returnOnAssetsTTM")]
        public decimal Roa { get; set; }

        [JsonPropertyName("netDebtToEBITDATTM")]
        public decimal DebtToEquity { get; set; }

        [JsonPropertyName("currentRatioTTM")]
        public decimal CurrentRatio { get; set; }

        [JsonPropertyName("freeCashFlowYieldTTM")]
        public decimal FreeCashFlowYield { get; set; }
    }
}
