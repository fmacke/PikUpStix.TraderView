using IKBR_Report_Puller.Domain;
using System.Collections.Generic;

namespace PikUpStix.TraderView.Interfaces
{
    /// <summary>
    /// Repository interface for TradeExecution Execution-related database operations
    /// </summary>
    public interface ITradeExecutionRepository
    {
        /// <summary>
        /// Inserts or updates trade executions from a report
        /// </summary>
        /// <param name="trades">List of trades to upsert</param>
        void UpsertTradeExecutions(List<TradeExecution> trades);

        /// <summary>
        /// Gets all trade executions ordered by order ID and date
        /// </summary>
        /// <returns>List of all trade executions</returns>
        List<TradeExecution> GetTradeExecutions();

        /// <summary>
        /// Inserts or updates today's trade confirmations
        /// </summary>
        /// <param name="tradeConfirms">List of trade confirmations to upsert</param>
        void UpsertTodayExecutions(List<TradeExecution> tradeConfirms);

        /// <summary>
        /// Gets aggregated trade summary for a specific order ID
        /// </summary>
        /// <param name="orderId">The order ID to retrieve</param>
        /// <returns>TradeExecution summary, or null if not found</returns>
        TradeSummary? GetTradeSummaryByOrderId(long orderId);

        /// <summary>
        /// Gets aggregated trade summary for a specific closing order ID
        /// </summary>
        /// <param name="closeOrderId">The closing order ID to retrieve</param>
        /// <returns>TradeExecution summary, or null if not found</returns>
        TradeSummary? GetTradeSummaryByCloseOrderId(long closeOrderId);

        /// <summary>
        /// Gets trade executions for a specific ConId and AccountId, ordered by trade date and time
        /// </summary>
        /// <param name="conid">The contract ID</param>
        /// <param name="accountId">The account ID</param>
        /// <returns>List of trade executions with date, quantity, and open/close indicator</returns>
        List<(DateTime TradeDate, decimal Quantity, string OpenCloseIndicator)> GetTradeExecutionsByConIdAndAccount(long? conid, string accountId);
    }
}
