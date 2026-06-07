using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IKBR_Report_Puller.Domain;

namespace IKBR_Report_Puller.Interfaces
{
    /// <summary>
    /// Service for retrieving and storing economic calendar data
    /// </summary>
    public interface IEconomicDataService
    {
        /// <summary>
        /// Retrieves economic calendar events for a date range, saves to file and database
        /// </summary>
        /// <param name="fromDate">Start date for calendar events</param>
        /// <param name="toDate">End date for calendar events</param>
        /// <returns>List of economic calendar events</returns>
        Task<List<EconomicCalendarEvent>> FetchAndSaveEconomicCalendarAsync(DateTime fromDate, DateTime toDate);
        Task FetchAndSaveChartData(List<HistoricalTrade> trades);
    }
}
