using Microsoft.Data.SqlClient;
using PikUpStix.TraderView.Domain;

namespace PikUpStix.TraderView.Interfaces
{
    public interface IPositionRepository
    {
        /// <summary>
        /// Gets all positions from the database
        /// </summary>
        /// <returns>List of positions</returns>
        List<Position> GetAllPositions();
        /// <summary>
        /// Gets all open positions from the database
        /// </summary>
        /// <returns>List of open positions</returns>
        List<Position> GetAllOpenPositions();
        /// <summary>
        /// Inserts a list of positions into the database
        /// </summary>
        /// <param name="positions">List of positions to insert</param>
        void UpsertPositions(List<Position> positions);
        /// <summary>
        /// Gets an open position by symbol and instrument ID
        /// </summary>
        /// <param name="symbol">The symbol of the instrument</param>
        /// <param name="instrumentId">The instrument ID</param>
        /// <returns>Open position or null if not found</returns>
        Position? GetOpenPosition(string symbol, int instrumentId);
        /// <summary>
        /// Gets an open position by symbol and instrument ID within a transaction
        /// </summary>
        /// <param name="connection">SQL connection</param>
        /// <param name="transaction">SQL transaction</param>
        /// <param name="symbol">The symbol of the instrument</param>
        /// <param name="instrumentId">The instrument ID</param>
        /// <returns>Open position or null if not found</returns>
        Position? GetOpenPosition(SqlConnection connection, SqlTransaction transaction, string symbol, int instrumentId);
        /// <summary>
        /// Creates a new position and returns its ID
        /// </summary>
        /// <param name="instrumentId">The instrument ID</param>
        /// <param name="symbol">The symbol</param>
        /// <param name="openDate">The open date</param>
        /// <returns>The ID of the newly created position</returns>
        int CreatePosition(int instrumentId, string symbol, DateTime openDate);
        /// <summary>
        /// Creates a new position and returns its ID within a transaction
        /// </summary>
        /// <param name="connection">SQL connection</param>
        /// <param name="transaction">SQL transaction</param>
        /// <param name="instrumentId">The instrument ID</param>
        /// <param name="symbol">The symbol</param>
        /// <param name="openDate">The open date</param>
        /// <returns>The ID of the newly created position</returns>
        int CreatePosition(SqlConnection connection, SqlTransaction transaction, int instrumentId, string symbol, DateTime openDate);
        /// <summary>
        /// Closes a position by setting its status to 'Closed' and close date
        /// </summary>
        /// <param name="connection">SQL connection</param>
        /// <param name="transaction">SQL transaction</param>
        /// <param name="positionId">The position ID to close</param>
        /// <param name="closeDate">The close date</param>
        void ClosePosition(SqlConnection connection, SqlTransaction transaction, int positionId, DateTime closeDate);
    }
}
    