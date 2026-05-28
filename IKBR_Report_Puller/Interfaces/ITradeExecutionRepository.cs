using IKBR_Report_Puller.Domain;
using System.Collections.Generic;

namespace IKBR_Report_Puller.Interfaces
{
    /// <summary>
    /// Repository interface for Trade Execution-related database operations
    /// </summary>
    public interface ITradeExecutionRepository
    {
        /// <summary>
        /// Inserts or updates trade executions from a report
        /// </summary>
        /// <param name="trades">List of trades to upsert</param>
        void UpsertTradeExecutions(List<Trade> trades);

        /// <summary>
        /// Gets all trade executions ordered by order ID and date
        /// </summary>
        /// <returns>List of all trade executions</returns>
        List<TradeExecution> GetTradeExecutions();

        /// <summary>
        /// Inserts or updates today's trade confirmations
        /// </summary>
        /// <param name="tradeConfirms">List of trade confirmations to upsert</param>
        void UpsertTodayExecutions(List<TradeConfirm> tradeConfirms);

        /// <summary>
        /// Gets aggregated trade summary for a specific order ID
        /// </summary>
        /// <param name="orderId">The order ID to retrieve</param>
        /// <returns>Trade summary, or null if not found</returns>
        TradeSummary? GetTradeSummaryByOrderId(long orderId);

        /// <summary>
        /// Gets aggregated trade summary for a specific closing order ID
        /// </summary>
        /// <param name="closeOrderId">The closing order ID to retrieve</param>
        /// <returns>Trade summary, or null if not found</returns>
        TradeSummary? GetTradeSummaryByCloseOrderId(long closeOrderId);
    }
}
