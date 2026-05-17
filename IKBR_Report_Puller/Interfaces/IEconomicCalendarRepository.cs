using System.Collections.Generic;
using IKBR_Report_Puller.Domain;

namespace IKBR_Report_Puller.Interfaces
{
    /// <summary>
    /// Repository for economic calendar database operations
    /// </summary>
    public interface IEconomicCalendarRepository
    {
        /// <summary>
        /// Inserts or updates economic calendar events in the database
        /// </summary>
        /// <param name="events">List of economic calendar events to upsert</param>
        void UpsertEconomicCalendarEvents(List<EconomicCalendarEvent> events);

        /// <summary>
        /// Retrieves all economic calendar events from the database
        /// </summary>
        /// <returns>List of all economic calendar events</returns>
        List<EconomicCalendarEvent> GetAllEvents();
    }
}
