using TraderView.Application.Interfaces.Repositories;
using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface IPositionService
    {
        Task<IReadOnlyList<Position>> GetOpenPositionsAsync();
        Task<Position?> GetOpenPositionAsync(int instrumentId);
        Task<int> CreatePositionAsync(int instrumentId, DateTime openDate, decimal openPrice);
        Task ClosePositionAsync(int positionId, DateTime closeDate);        
        Task UpsertPositionsAsync(IReadOnlyList<Position> positions);
    }
}
