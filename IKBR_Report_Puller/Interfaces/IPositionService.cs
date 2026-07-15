using PikUpStix.TraderView.Domain;

namespace PikUpStix.TraderView.Interfaces
{
    /// <summary>
    /// Service interface for Position operations
    /// </summary>
    public interface IPositionService
    {
        /// <summary>
        /// Gets all positions asynchronously
        /// </summary>
        /// <returns>List of all positions</returns>
        Task<List<Position>> GetAllPositionsAsync();

        /// <summary>
        /// Gets all open positions asynchronously
        /// </summary>
        /// <returns>List of all open positions</returns>
        Task<List<Position>> GetAllOpenPositionsAsync();

        /// <summary>
        /// Gets an open position by symbol and instrument ID asynchronously
        /// </summary>
        /// <param name="symbol">The symbol of the instrument</param>
        /// <param name="instrumentId">The instrument ID</param>
        /// <returns>Open position or null if not found</returns>
        Task<Position?> GetOpenPositionAsync(string symbol, int instrumentId);

        /// <summary>
        /// Creates a new position asynchronously
        /// </summary>
        /// <param name="instrumentId">The instrument ID</param>
        /// <param name="symbol">The symbol</param>
        /// <param name="openDate">The open date</param>
        /// <returns>The ID of the newly created position</returns>
        Task<int> CreatePositionAsync(int instrumentId, string symbol, DateTime openDate);

        /// <summary>
        /// Inserts or updates a list of positions asynchronously
        /// </summary>
        /// <param name="positions">List of positions to upsert</param>
        Task UpsertPositionsAsync(List<Position> positions);
    }
}
