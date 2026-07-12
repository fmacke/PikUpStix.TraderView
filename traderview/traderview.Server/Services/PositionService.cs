using PikUpStix.TraderView.Domain;
using PikUpStix.TraderView.Interfaces;

namespace traderview.Server.Services
{
    /// <summary>
    /// Service for Position operations
    /// </summary>
    public class PositionService : IPositionService
    {
        private readonly IPositionRepository _positionRepository;
        private readonly ILogger<PositionService> _logger;

        public PositionService(
            IPositionRepository positionRepository,
            ILogger<PositionService> logger)
        {
            _positionRepository = positionRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets all positions asynchronously
        /// </summary>
        public async Task<List<Position>> GetAllPositionsAsync()
        {
            try
            {
                return await Task.Run(() => _positionRepository.GetAllPositions());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all positions");
                throw;
            }
        }

        /// <summary>
        /// Gets all open positions asynchronously
        /// </summary>
        public async Task<List<Position>> GetAllOpenPositionsAsync()
        {
            try
            {
                return await Task.Run(() => _positionRepository.GetAllOpenPositions());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all open positions");
                throw;
            }
        }

        /// <summary>
        /// Gets an open position by symbol and instrument ID asynchronously
        /// </summary>
        public async Task<Position?> GetOpenPositionAsync(string symbol, int instrumentId)
        {
            try
            {
                return await Task.Run(() => _positionRepository.GetOpenPosition(symbol, instrumentId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching open position for symbol {Symbol} and instrument {InstrumentId}", symbol, instrumentId);
                throw;
            }
        }

        /// <summary>
        /// Creates a new position asynchronously
        /// </summary>
        public async Task<int> CreatePositionAsync(int instrumentId, string symbol, DateTime openDate)
        {
            try
            {
                return await Task.Run(() => _positionRepository.CreatePosition(instrumentId, symbol, openDate));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating position for instrument {InstrumentId} with symbol {Symbol}", instrumentId, symbol);
                throw;
            }
        }

        /// <summary>
        /// Inserts or updates a list of positions asynchronously
        /// </summary>
        public async Task UpsertPositionsAsync(List<Position> positions)
        {
            try
            {
                await Task.Run(() => _positionRepository.UpsertPositions(positions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting {Count} positions", positions?.Count ?? 0);
                throw;
            }
        }
    }
}
