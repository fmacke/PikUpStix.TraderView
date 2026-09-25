using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for economic calendar database operations
    /// </summary>
    public interface IEconomicCalendarRepository : IRepository<EconomicCalendar>
    {
        /// <summary>
        /// Inserts or updates economic calendar events in the database
        /// </summary>
        /// <param name="events">List of economic calendar events to upsert</param>
        Task UpsertEconomicCalendarEventsAsync(List<EconomicCalendar> events);

        /// <summary>
        /// Retrieves all economic calendar events from the database, ordered by date descending
        /// </summary>
        /// <returns>List of all economic calendar events ordered by date descending</returns>
        Task<List<EconomicCalendar>> GetAllEventsAsync();
    }
}
