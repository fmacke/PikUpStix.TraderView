namespace TraderView.Domain.Entities.FMP
{
    public class CanSlimScreenerCriteria
    {
        #region Universe & Liquidity (Institutional Quality)

        /// <summary>
        /// Minimum share price. O'Neil avoids low-priced/penny stocks; 
        /// IBD baseline is $15+ to ensure institutional fund sponsorship.
        /// </summary>
        public decimal MinPrice { get; set; } = 15.00m;

        /// <summary>
        /// Minimum 50-day Average Daily Volume (ADV).
        /// Standard CAN SLIM floor is 400k shares; top institutional leaders typically trade >= 500k-1M shares/day.
        /// </summary>
        public long MinVolume { get; set; } = 400_000;

        /// <summary>
        /// Minimum average daily dollar volume (Price * ADV).
        /// Ensures at least $10M-$20M turnover/day so funds can enter/exit without extreme slippage.
        /// </summary>
        public decimal MinDollarVolume { get; set; } = 10_000_000m;

        /// <summary>
        /// Minimum Market Capitalisation ($500M micro-cap cutoff; mid/large growth sweet spot is $1B - $50B).
        /// </summary>
        public long MinMarketCap { get; set; } = 500_000_000;

        /// <summary>
        /// Exchanges to include (NASDAQ, NYSE, AMEX).
        /// </summary>
        public string Exchanges { get; set; } = "NASDAQ,NYSE,AMEX";

        #endregion

        #region 'C' - Current Quarterly Earnings & Sales

        /// <summary>
        /// Minimum Year-over-Year (YoY) Diluted EPS growth for the most recent quarter.
        /// Minimum: 20-25%; True market leaders often print 50% to 100%+.
        /// </summary>
        public decimal MinCurrentQuarterEpsGrowthPercent { get; set; } = 25.0m;

        /// <summary>
        /// Minimum YoY Sales/Revenue growth for the most recent quarter.
        /// Confirms earnings quality is driven by top-line demand, not just margin cutting.
        /// </summary>
        public decimal MinCurrentQuarterRevGrowthPercent { get; set; } = 20.0m;

        /// <summary>
        /// Require latest quarter EPS YoY growth to be higher than the prior quarter's YoY growth.
        /// </summary>
        public bool RequireEpsAcceleration { get; set; } = false; // Enabled for strict high-conviction scans

        #endregion

        #region 'A' - Annual Earnings & Profitability

        /// <summary>
        /// 3-Year Compound Annual Growth Rate (CAGR) for EPS.
        /// O'Neil benchmark: 25% minimum annual growth over the prior 3 years.
        /// </summary>
        public decimal MinAnnualEpsCagrPercent { get; set; } = 25.0m;

        /// <summary>
        /// Minimum Trailing Twelve Months (TTM) Return on Equity (ROE).
        /// O'Neil's key filter to separate elite capital compounders from mediocre firms.
        /// </summary>
        public decimal MinReturnOnEquityPercent { get; set; } = 17.0m;

        /// <summary>
        /// Require EPS to have grown monotonically over the last 3 fiscal years (Y0 > Y1 > Y2).
        /// </summary>
        public bool RequireConsecutiveAnnualGrowth { get; set; } = true;

        #endregion

        #region 'L' & 'N' - Relative Strength & Technical Stage

        /// <summary>
        /// Minimum Relative Strength (RS) Rating (1-99 percentile scale).
        /// O'Neil rule: Never buy a stock with an RS under 70; IBD Leaderboard focuses on RS 80-99.
        /// </summary>
        public int MinRelativeStrengthRating { get; set; } = 80;

        /// <summary>
        /// Maximum allowable distance below the 52-Week High (%).
        /// CAN SLIM leaders break out from constructive bases within 10% to 15% of all-time/52-week highs.
        /// </summary>
        public decimal MaxDistanceBelow52WeekHighPercent { get; set; } = 15.0m;

        /// <summary>
        /// Stock price must trade above its 50-day Simple Moving Average (SMA).
        /// </summary>
        public bool RequirePriceAbove50Sma { get; set; } = true;

        /// <summary>
        /// Stock price must trade above its 200-day Simple Moving Average (SMA).
        /// </summary>
        public bool RequirePriceAbove200Sma { get; set; } = true;

        /// <summary>
        /// 50-day SMA must trade above 200-day SMA (Confirms Stan Weinstein Stage 2 uptrend).
        /// </summary>
        public bool Require50SmaAbove200Sma { get; set; } = true;

        #endregion

        #region 'S' - Supply & Demand (Volume Characteristics)

        /// <summary>
        /// 50-Day Up/Down Volume Ratio.
        /// Calculated as (Volume on Up Days / Volume on Down Days over 50 trading days).
        /// Values > 1.0 indicate net institutional accumulation; >= 1.2 is strong accumulation.
        /// </summary>
        public decimal MinUpDownVolumeRatio { get; set; } = 1.0m;

        #endregion

        #region Engine & Execution Performance

        /// <summary>
        /// Maximum concurrent tasks when querying detailed statements across the candidate universe.
        /// Set between 6-10 to prevent FMP 429 throttling while maintaining rapid screening execution.
        /// </summary>
        public int MaxDegreeOfParallelism { get; set; } = 8;

        /// <summary>
        /// Maximum candidate count returned from the Stage 1 broad market screener.
        /// </summary>
        public int Stage1UniverseLimit { get; set; } = 1000;

        #endregion
    }
}
