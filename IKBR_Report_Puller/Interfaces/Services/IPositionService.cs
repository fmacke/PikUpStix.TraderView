using TraderView.Application.Interfaces.Repositories;
using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface IPositionService
    {
        Task<Position?> GetPositionByIdAsync(int positionId);
        Task<List<Position>> GetAllPositionsAsync(bool loadTradeExecutions);
        Task<IReadOnlyList<Position>> GetOpenPositionsAsync();
        Task<Position?> GetOpenPositionAsync(int instrumentId);
        Task<int> CreatePositionAsync(int instrumentId, DateTime openDate, decimal openPrice, string marketRegime);
        Task ClosePositionAsync(int positionId, DateTime closeDate);        
        Task UpsertPositionsAsync(IReadOnlyList<Position> positions, string currentMarketRegime);
    }
}
