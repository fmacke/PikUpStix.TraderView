using TraderView.Domain.Entities;
using TraderView.Application.Interfaces.Persistence;

namespace TraderView.Application.Interfaces.Repositories;

/// <summary>
/// Async repository interface for Position-related operations
/// </summary>
public interface IPositionRepository : IRepository<Position>
{
    Task<Position?> GetOpenPositionAsync(int instrumentId);

    Task<int> CreatePositionAsync(int instrumentId, string symbol, DateTime openDate, decimal openPrice);

    Task<List<Position>> GetAllPositionsAsync();

    Task<List<Position>> GetOpenPositionsAsync();

    Task UpsertPositionsAsync(List<Position> positions);
    Task UpdatePositionAsync(int positionId, DateTime dateTime, decimal price, string status);
    Task ClosePositionAsync(int positionId, DateTime closeDate);
}
