using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface IPositionService
    {
        Task<IReadOnlyList<Position>> GetOpenPositionsAsync();
        Task<int> CreatePositionAsync(int instrumentId, string symbol, DateTime openDate, decimal openPrice);
        Task ClosePositionAsync(int positionId, DateTime closeDate);
        Task<Position?> GetOpenPositionAsync(int instrumentId);
    }
}
