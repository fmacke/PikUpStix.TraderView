namespace TraderView.Domain.Backtest
{
    /// <summary>
    /// Represents a single round-trip trade executed during backtesting.
    /// </summary>
    public class BacktestTrade
    {
        /// <summary>
        /// Bar index when entry order was filled
        /// </summary>
        public int EntryBarIndex { get; set; }

        /// <summary>
        /// Date of entry
        /// </summary>
        public DateTime EntryDate { get; set; }

        /// <summary>
        /// Actual entry price (may differ from order due to slippage)
        /// </summary>
        public double EntryPrice { get; set; }

        /// <summary>
        /// Long or Short
        /// </summary>
        public PositionDirection Direction { get; set; }

        /// <summary>
        /// Quantity entered
        /// </summary>
        public double Quantity { get; set; }

        /// <summary>
        /// Bar index when exit order was filled
        /// </summary>
        public int? ExitBarIndex { get; set; }

        /// <summary>
        /// Date of exit (null if still open)
        /// </summary>
        public DateTime? ExitDate { get; set; }

        /// <summary>
        /// Actual exit price
        /// </summary>
        public double? ExitPrice { get; set; }

        /// <summary>
        /// Type of exit: ProfitTarget, StopLoss, or Liquidation
        /// </summary>
        public ExitType? ExitType { get; set; }

        /// <summary>
        /// Gross P&L in dollars (before commission)
        /// </summary>
        public double GrossProfit { get; set; }

        /// <summary>
        /// Total commission charged
        /// </summary>
        public double Commission { get; set; }

        /// <summary>
        /// Net P&L after commission
        /// </summary>
        public double NetProfit { get; set; }

        /// <summary>
        /// Return as percentage: (NetProfit / (EntryPrice * Quantity)) * 100
        /// </summary>
        public double ReturnPercent { get; set; }

        /// <summary>
        /// Maximum Favorable Excursion: best unrealized P&L during hold
        /// </summary>
        public double MaxFavorableExcursion { get; set; }

        /// <summary>
        /// Maximum Adverse Excursion: worst unrealized P&L during hold
        /// </summary>
        public double MaxAdverseExcursion { get; set; }

        /// <summary>
        /// Number of bars held (ExitBarIndex - EntryBarIndex)
        /// </summary>
        public int? BarsHeld => ExitBarIndex - EntryBarIndex;
    }

    public enum ExitType
    {
        ProfitTarget = 0,
        StopLoss = 1,
        Liquidation = 2,  // End of backtest or manual close
    }
}
