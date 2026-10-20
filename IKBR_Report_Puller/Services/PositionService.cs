using TraderView.Domain.Entities;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;

namespace PikUpStix.TraderView.Services
{
    public class PositionService : IPositionService
    {
        private readonly IPositionRepository _positionRepository;

        public PositionService(IPositionRepository positionRepository)
        {
            _positionRepository = positionRepository ?? throw new ArgumentNullException(nameof(positionRepository));
        }
        async Task IPositionService.ClosePositionAsync(int positionId, DateTime closeDate)
        {
            var position = await _positionRepository.GetByIdAsync(positionId);
            if (position == null)
            {
                throw new InvalidOperationException($"Position with ID {positionId} not found.");
            }
            position.CloseDate = closeDate;
            position.Status = "Closed";
            await _positionRepository.UpdateAsync(position);
        }
        async Task<int> IPositionService.CreatePositionAsync(int instrumentId, DateTime openDate, decimal openPrice, string marketRegime)
        {
            var positionIdTask = await _positionRepository.CreatePositionAsync(instrumentId, openDate, openPrice, marketRegime);
            return positionIdTask;
        }
        async Task<Position?> IPositionService.GetPositionByIdAsync(int positionId)
        {
            return await _positionRepository.GetByIdAsync(positionId);
        }
        async Task<Position?> IPositionService.GetOpenPositionAsync(int instrumentId)
        {
            return await _positionRepository.GetOpenPositionAsync(instrumentId);
        }
        async Task<IReadOnlyList<Position>> IPositionService.GetOpenPositionsAsync()
        {
            return await Task.Run(() => _positionRepository.GetOpenPositionsAsync());
        }
        async Task<List<Position>> IPositionService.GetAllPositionsAsync()
        {
            return await _positionRepository.GetAllPositionsAsync();
        }
        async Task IPositionService.UpsertPositionsAsync(IReadOnlyList<Position> positions, string currentMarketRegime)
        {
            if (positions == null || !positions.Any())
            {
                Console.WriteLine("No positions to upsert.");
                return;
            }

            int insertedCount = 0;
            int updatedCount = 0;

            foreach (var position in positions)
            {
                // Ensure instrument exists before upserting position
                if (position.Id == 0)
                {
                    await _positionRepository.CreatePositionAsync(position.InstrumentId, position.OpenDate, position.LastReportedPrice ?? 0m, currentMarketRegime);
                    insertedCount++;
                }
                else
                {
                    await _positionRepository.UpdatePositionAsync(position.Id, DateTime.Now, position.LastReportedPrice ?? 0m, "Open");
                    updatedCount++;
                }
            }
            Console.WriteLine($"Successfully processed {positions.Count} positions: {insertedCount} inserted, {updatedCount} updated.");
        }
    }
}
