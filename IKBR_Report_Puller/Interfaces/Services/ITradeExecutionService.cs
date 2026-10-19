using TraderView.Application.Interfaces.Repositories;
using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface ITradeExecutionService
    {
        Task<List<TradeExecution>> GetTradeExecutionsAsync();
        Task<List<TradeExecution>> GetTradeExecutionsByPositionIdAsync(int positionId);
        Task<TradeSummary?> GetTradeSummaryByPositionIdAsync(int positionId);
        Task UpsertTradeExecutionsAsync(IReadOnlyList<TradeExecution> trades);
        Task UpsertTradeConfirmationsAsync(IReadOnlyList<TradeConfirm> tradeConfirms, string currentMarketRegime);
    }
}
