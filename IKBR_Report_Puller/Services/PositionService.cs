using PikUpStix.TraderView.Domain;
using PikUpStix.TraderView.Interfaces;

namespace PikUpStix.TraderView.Services
{
    /// <summary>
    /// Service for Position operations
    /// </summary>
    public class PositionService : IPositionService
    {
        private readonly IPositionRepository _positionRepository;

        public PositionService(IPositionRepository positionRepository)
        {
            _positionRepository = positionRepository;
        }

        /// <summary>
        /// Gets all open positions asynchronously
        /// </summary>
        public async Task<List<Position>> GetAllOpenPositionsAsync()
        {
            return await Task.Run(() => _positionRepository.GetAllOpenPositions());
        }

        /// <summary>
        /// Gets an open position by symbol and instrument ID asynchronously
        /// </summary>
        public async Task<Position?> GetOpenPositionAsync(string symbol, int instrumentId)
        {
            return await Task.Run(() => _positionRepository.GetOpenPosition(symbol, instrumentId));
        }

        /// <summary>
        /// Creates a new position asynchronously
        /// </summary>
        public async Task<int> CreatePositionAsync(int instrumentId, string symbol, DateTime openDate, decimal openPrice)
        {
            return await Task.Run(() => _positionRepository.CreatePosition(instrumentId, symbol, openDate, openPrice));
        }
    }
}
