namespace TraderView.Domain.Backtest
{
    /// <summary>
    /// Complete summary of a backtesting run.
    /// Suitable for rendering in dashboard and React client.
    /// </summary>
    public class BacktestResult
    {
        /// <summary>
        /// Instrument being backtested
        /// </summary>
        public string InstrumentName { get; set; } = null!;

        /// <summary>
        /// Starting cash available for trading
        /// </summary>
        public double InitialCapital { get; set; }

        /// <summary>
        /// Ending capital after all trades
        /// </summary>
        public double FinalCapital { get; set; }

        /// <summary>
        /// Total return in dollars
        /// </summary>
        public double TotalReturn { get; set; }

        /// <summary>
        /// Total return as percentage
        /// </summary>
        public double TotalReturnPercent { get; set; }

        /// <summary>
        /// Maximum drawdown from peak equity
        /// </summary>
        public double MaxDrawdown { get; set; }

        /// <summary>
        /// Maximum drawdown as percentage
        /// </summary>
        public double MaxDrawdownPercent { get; set; }

        /// <summary>
        /// Total number of trades executed
        /// </summary>
        public int TotalTrades { get; set; }

        /// <summary>
        /// Number of winning trades
        /// </summary>
        public int WinningTrades { get; set; }

        /// <summary>
        /// Number of losing trades
        /// </summary>
        public int LosingTrades { get; set; }

        /// <summary>
        /// Win rate: WinningTrades / TotalTrades
        /// </summary>
        public double WinRate { get; set; }

        /// <summary>
        /// Average profit per winning trade
        /// </summary>
        public double AvgWin { get; set; }

        /// <summary>
        /// Average loss per losing trade
        /// </summary>
        public double AvgLoss { get; set; }

        /// <summary>
        /// Profit factor: GrossWins / GrossLosses
        /// </summary>
        public double ProfitFactor { get; set; }

        /// <summary>
        /// Total commission paid
        /// </summary>
        public double TotalCommission { get; set; }

        /// <summary>
        /// Date range: from
        /// </summary>
        public DateTime BacktestStartDate { get; set; }

        /// <summary>
        /// Date range: to
        /// </summary>
        public DateTime BacktestEndDate { get; set; }

        /// <summary>
        /// All executed trades
        /// </summary>
        public List<BacktestTrade> Trades { get; set; } = new();

        /// <summary>
        /// Daily equity values for charting
        /// </summary>
        public List<EquityPoint> EquityCurve { get; set; } = new();

        /// <summary>
        /// Strategy parameters or metadata
        /// </summary>
        public Dictionary<string, string> StrategyMetadata { get; set; } = new();
    }

    /// <summary>
    /// Single point on the equity curve (for charting).
    /// </summary>
    public class EquityPoint
    {
        public DateTime Date { get; set; }
        public double Equity { get; set; }
        public double DrawdownPercent { get; set; }
    }
}
