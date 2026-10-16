using TraderView.Domain.Entities;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;

namespace PikUpStix.TraderView.Services
{
    public class PositionService : IPositionService
    {
        private readonly IPositionRepository _positionRepository;
        private readonly ITradingReportService _marketRegimeService;

        public PositionService(IPositionRepository positionRepository, ITradingReportService marketRegimeService)
        {
            _positionRepository = positionRepository ?? throw new ArgumentNullException(nameof(positionRepository));
            _marketRegimeService = marketRegimeService ?? throw new ArgumentNullException(nameof(marketRegimeService));
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
        async Task<int> IPositionService.CreatePositionAsync(int instrumentId, DateTime openDate, decimal openPrice)
        {
            var marketRegime = await _marketRegimeService.DetermineRegimeAsync(openDate, new CancellationToken());
            var positionIdTask = await _positionRepository.CreatePositionAsync(instrumentId, openDate, openPrice, marketRegime);
            return positionIdTask;
        }
        async Task<Position?> IPositionService.GetOpenPositionAsync(int instrumentId)
        {
            return await _positionRepository.GetOpenPositionAsync(instrumentId);
        }
        async Task<IReadOnlyList<Position>> IPositionService.GetOpenPositionsAsync()
        {
            return await Task.Run(() => _positionRepository.GetOpenPositionsAsync());
        }
        async Task IPositionService.UpsertPositionsAsync(IReadOnlyList<Position> positions)
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
                    var marketRegime = await _marketRegimeService.DetermineRegimeAsync(position.OpenDate, new CancellationToken());
                    await _positionRepository.CreatePositionAsync(position.InstrumentId, position.OpenDate, position.LastReportedPrice ?? 0m, marketRegime);
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
