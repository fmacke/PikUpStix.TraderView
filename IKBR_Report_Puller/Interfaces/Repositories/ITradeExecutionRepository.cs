using TraderView.Domain.Entities;
namespace TraderView.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for TradeExecution Execution-related database operations
    /// </summary>
    public interface ITradeExecutionRepository
    {
        Task<List<TradeExecution>> GetTradeExecutionsByPositionIdAsync(int positionId);
        Task<TradeSummary?> GetTradeSummaryByPositionIdAsync(int positionId);
        Task<List<TradeExecution>> GetTradeExecutionsAsync();
        Task<List<(DateTime TradeDate, decimal Quantity, string OpenCloseIndicator)>> GetTradeExecutionsByConIdAndAccountAsync(long? conid, string accountId);        
        Task InsertTradeConfirmationsAsync(List<TradeConfirm> tradeConfirms);
        Task UpsertTradeExecutionsAsync(List<TradeExecution> trades);

    }
}
