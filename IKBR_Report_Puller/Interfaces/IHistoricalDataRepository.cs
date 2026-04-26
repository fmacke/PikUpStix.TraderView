using IKBR_Report_Puller.Domain;
using System;
using System.Collections.Generic;

namespace IKBR_Report_Puller.Interfaces
{
    /// <summary>
    /// Repository interface for Historical Data (chart data) operations
    /// </summary>
    public interface IHistoricalDataRepository
    {
        /// <summary>
        /// Inserts chart data bars for a given instrument, skipping duplicates
        /// </summary>
        /// <param name="instrumentId">The instrument ID as string</param>
        /// <param name="bars">List of historical price bars to insert</param>
        void UpdateHistoricalData(string instrumentId, List<Bar> bars);

        /// <summary>
        /// Gets missing date ranges for historical data for a given instrument and date range
        /// </summary>
        /// <param name="instrumentId">The instrument ID</param>
        /// <param name="startDate">Start date of the range</param>
        /// <param name="endDate">End date of the range</param>
        /// <returns>List of date ranges that are missing data</returns>
        List<(DateTime startDate, DateTime endDate)> GetMissingDateRanges(int instrumentId, DateTime startDate, DateTime endDate);
    }
}
