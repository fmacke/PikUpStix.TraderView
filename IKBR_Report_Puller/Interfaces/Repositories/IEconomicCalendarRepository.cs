using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Repositories
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
        void UpsertEconomicCalendarEvents(List<EconomicCalendar> events);

        /// <summary>
        /// Retrieves all economic calendar events from the database
        /// </summary>
        /// <returns>List of all economic calendar events</returns>
        List<EconomicCalendar> GetAllEvents();
    }
}
