using PikUpStix.TraderView.Domain;

namespace PikUpStix.TraderView.Interfaces
{
    public interface ITradeExecutionService
    {
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
        /// <param name="openPrice">The open price</param>
        /// <returns>The ID of the newly created position</returns>
        Task<int> CreatePositionAsync(int instrumentId, string symbol, DateTime openDate, decimal openPrice);
        /// <summary>
        /// Gets all open positions from the database
        /// </summary>
        /// <returns>List of all open positions</returns>
        Task<List<Position>> GetOpenPositionsAsync();
    }
}
