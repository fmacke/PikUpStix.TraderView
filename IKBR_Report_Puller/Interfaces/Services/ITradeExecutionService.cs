using TraderView.Application.Interfaces.Repositories;
using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface ITradeExecutionService
    {
        Task<List<TradeExecution>> GetTradeExecutionsAsync();
        Task<List<TradeExecution>> GetTradeExecutionsByPositionIdAsync(int positionId);
        Task<List<(DateTime TradeDate, decimal Quantity, string OpenCloseIndicator)>> GetTradeExecutionsByConIdAndAccountAsync(long? conid, string accountId);
        Task<TradeSummary?> GetTradeSummaryByPositionIdAsync(int positionId);
        Task UpsertTradeExecutionsAsync(IReadOnlyList<TradeExecution> trades);
        Task UpsertTradeConfirmationsAsync(IReadOnlyList<TradeConfirm> tradeConfirms, string currentMarketRegime);
    }
}
