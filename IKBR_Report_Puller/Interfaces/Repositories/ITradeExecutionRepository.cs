using TraderView.Domain.Entities;
namespace TraderView.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for TradeExecution Execution-related database operations
    /// </summary>
    public interface ITradeExecutionRepository
    {
        /// <summary>
        /// Gets all positions from the database
        /// </summary>
        /// <returns>List of all positions</returns>
        List<Position> GetAllPositions();
        /// <summary>
        /// Gets all open positions from the database
        /// </summary>
        /// <returns>List of all open positions</returns>
        List<Position> GetOpenPositions();
        /// <summary>
        /// Gets trade executions for a specific position ID
        /// </summary>
        /// <param name="positionId">The position ID</param>
        /// <returns>List of trade executions for the position</returns>
        List<TradeExecution> GetByPositionId(int positionId);
        /// <summary>
        /// Gets trade summary for a specific position ID
        /// </summary>
        /// <param name="positionId">The position ID</param>
        /// <returns>Trade summary for the position</returns>
        TradeSummary? GetTradeSummaryByPositionId(int positionId);
        /// <summary>
        /// Gets all trade executions ordered by order ID and date
        /// </summary>
        /// <returns>List of all trade executions</returns>
        List<TradeExecution> GetTradeExecutions();
        /// <summary>
        /// Inserts or updates trade executions from a report
        /// </summary>
        /// <param name="trades">List of trades to upsert</param>
        /// TradeSummary? GetTradeSummaryByPositionId(int positionId);
        /// <summary>
        /// Gets trade executions for a specific ConId and AccountId, ordered by trade date and time
        /// </summary>
        /// <param name="conid">The contract ID</param>
        /// <param name="accountId">The account ID</param>
        /// <returns>List of trade executions with date, quantity, and open/close indicator</returns>        
        List<(DateTime TradeDate, decimal Quantity, string OpenCloseIndicator)> GetTradeExecutionsByConIdAndAccount(long? conid, string accountId);        
        /// <summary>
        /// Inserts today's trade confirmations
        /// </summary>
        /// <param name="tradeConfirms">List of trade confirmations to insert</param>
        void InsertTradeConfirmations(List<TradeConfirm> tradeConfirms);
        /// <summary>
        /// Inserts or updates positions
        /// </summary>
        /// <param name="positions">List of positions to upsert</param>
        void UpsertPositions(List<Position> positions);
        /// <summary>
        /// Inserts or updates trade executions
        /// </summary>
        /// <param name="trades">List of trade executions to upsert</param>
        void UpsertTradeExecutions(List<TradeExecution> trades);
        
    }
}
