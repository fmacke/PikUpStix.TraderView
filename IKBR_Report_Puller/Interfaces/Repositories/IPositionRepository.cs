using TraderView.Domain.Entities;
using TraderView.Application.Interfaces.Persistence;

namespace TraderView.Application.Interfaces.Repositories;

/// <summary>
/// Async repository interface for Position-related operations
/// </summary>
public interface IPositionRepository : IRepository<Position>
{
    Task<Position?> GetOpenPositionAsync(int instrumentId);
    Task<int> CreatePositionAsync(int instrumentId, DateTime openDate, decimal openPrice, string marketRegime);
    Task<List<Position>> GetAllPositionsAsync(bool loadTradeExecutions);
    Task<List<Position>> GetOpenPositionsAsync();
    Task UpdatePositionAsync(int positionId, DateTime dateTime, decimal price, string status);
    Task ClosePositionAsync(int positionId, DateTime closeDate);
}
