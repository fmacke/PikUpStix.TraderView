namespace TraderView.Domain.Backtest
{
    /// <summary>
    /// Represents a pending order intent from the strategy.
    /// The strategy decides entry, stop-loss, and profit target prices.
    /// </summary>
    public class OrderIntent
    {
        /// <summary>
        /// Type of order: Entry, StopLoss, ProfitTarget
        /// </summary>
        public OrderType OrderType { get; set; }

        /// <summary>
        /// Price at which this order should trigger
        /// </summary>
        public double TriggerPrice { get; set; }

        /// <summary>
        /// Size of the position (number of contracts/shares)
        /// </summary>
        public double PositionSize { get; set; }

        /// <summary>
        /// Entry direction: Long or Short
        /// </summary>
        public PositionDirection Direction { get; set; }

        /// <summary>
        /// Order placement bar index (for tracking)
        /// </summary>
        public int PlacedAtBarIndex { get; set; }

        /// <summary>
        /// Associated with a specific position ID (optional, for targeting stop/target)
        /// </summary>
        public int? RelatedPositionId { get; set; }
    }

    public enum OrderType
    {
        Entry = 0,
        StopLoss = 1,
        ProfitTarget = 2,
    }

    public enum PositionDirection
    {
        Long = 1,
        Short = -1,
    }
}
