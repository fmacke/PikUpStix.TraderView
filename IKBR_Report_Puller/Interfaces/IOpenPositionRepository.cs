using IKBR_Report_Puller.Domain;
using System;
using System.Collections.Generic;

namespace PikUpStix.TraderView.Interfaces
{
    /// <summary>
    /// Repository interface for Open Position database operations
    /// </summary>
    public interface IOpenPositionRepository
    {
        /// <summary>
        /// Gets all open positions from the database
        /// </summary>
        /// <returns>List of all open positions</returns>
        List<OpenPosition> GetAllOpenPositions();

        /// <summary>
        /// Inserts open positions from a report
        /// </summary>
        /// <param name="whenGenerated">Date when the report was generated</param>
        /// <param name="openPositions">List of open positions to insert</param>
        void InsertOpenPositions(DateTime whenGenerated, List<OpenPosition> openPositions);

        /// <summary>
        /// Gets instrument names for open positions that are missing instrument data
        /// </summary>
        /// <param name="openPositions">List of open positions to check</param>
        /// <returns>List of tuples containing security ID, listing exchange, and symbol</returns>
        List<(string securityID, string listingExchange, string symbol)> GetOpenPositionInstrumentNames(List<OpenPosition> openPositions);
    }
}
