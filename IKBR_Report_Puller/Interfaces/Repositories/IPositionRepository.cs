using TraderView.Domain.Entities;
using TraderView.Application.Interfaces.Persistence;

namespace TraderView.Application.Interfaces.Repositories;

/// <summary>
/// Async repository interface for Position-related operations
/// </summary>
public interface IPositionRepository : IRepository<Position>
{
    Task<Position?> GetOpenPositionAsync(string symbol, int instrumentId);

    Task<int> CreatePositionAsync(int instrumentId, string symbol, DateTime openDate, decimal openPrice);

    Task<List<Position>> GetAllPositionsAsync();

    Task<List<Position>> GetOpenPositionsAsync();

    Task UpsertPositionsAsync(List<Position> positions);
}
