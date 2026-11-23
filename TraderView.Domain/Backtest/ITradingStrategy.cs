using TraderView.Domain.Entities;

namespace TraderView.Domain.Backtest
{
    /// <summary>
    /// Pure domain interface for a trading strategy.
    /// 
    /// Contract:
    /// - ZERO I/O: no database calls, file reads, or network requests
    /// - ZERO side effects: this is a pure function (given the same bars, always return the same orders)
    /// - ZERO DateTime.Now: strategy receives historical bar timestamps
    /// - Look-ahead safe: only receives bars[0..i] up to current bar i (never future bars)
    /// 
    /// The exact same strategy class will later evaluate live bars without modification.
    /// </summary>
    public interface ITradingStrategy
    {
        /// <summary>
        /// Evaluate strategy on the given historical lookback bars.
        /// 
        /// Called once per bar in chronological order.
        /// The strategy has access to all prior bars and the current bar (bars[0] = oldest, bars[count-1] = current).
        /// 
        /// Must not access bars beyond index (count - 1).
        /// </summary>
        /// <param name="bars">
        /// Historical bars up to and including the current bar.
        /// Index 0 = oldest, Index (count-1) = current/most recent bar.
        /// </param>
        /// <param name="currentBarIndex">
        /// The index of the current bar in the overall backtest (for logging/tracking).
        /// This is not an index into the bars list; it's provided for reference.
        /// </param>
        /// <returns>
        /// List of orders to place for the next bar.
        /// Empty list = no action.
        /// Multiple orders allowed (e.g., entry + stop-loss + profit-target placed simultaneously).
        /// </returns>
        List<OrderIntent> Evaluate(IReadOnlyList<Bar> bars, int currentBarIndex);

        /// <summary>
        /// Strategy name for logging and reporting.
        /// </summary>
        string StrategyName { get; }
    }
}
