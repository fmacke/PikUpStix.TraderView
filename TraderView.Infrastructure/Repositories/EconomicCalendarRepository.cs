using TraderView.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using TraderView.Application.Specifications;
using TraderView.Application.Specifications.EconomicCalendars;
using TraderView.Domain.Entities;
using TraderView.Infrastructure.DbContexts;

namespace TraderView.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for economic calendar database operations using Entity Framework Core
    /// </summary>
    public class EconomicCalendarRepository : EfBaseRepository<EconomicCalendar>, IEconomicCalendarRepository
    {
        private DbContext _context;
        public EconomicCalendarRepository(AppDbContext db) : base(db)
        {
            _context = db;
        }

        /// <summary>
        /// Inserts or updates economic calendar events in the database
        /// </summary>
        public async Task UpsertEconomicCalendarEventsAsync(List<EconomicCalendar> events)
        {
            if (events == null || events.Count == 0)
            {
                Console.WriteLine("No economic calendar events to insert.");
                return;
            }

            // Determine the incoming date range so we fetch all potentially matching
            // rows from the database. Using the full DateTime (not Date) preserves
            // the time component so later comparisons against the incoming
            // event.Timestamp succeed.
            var minDate = events.Min(e => e.Date);
            var maxDate = events.Max(e => e.Date);

            try
            {
                // Get existing events up to the earliest incoming entry date so we only
                // compare against relevant rows.
                // Fetch existing events that fall within the incoming range (inclusive)
                // so we can accurately match by full Date/Time, Country and Event.
                var existingEvents = await _context.Set<EconomicCalendar>()
                    .Where(e => e.Date >= minDate && e.Date <= maxDate)
                    .ToListAsync();
                var now = DateTime.UtcNow;

                foreach (var evt in events)
                {
                    // Find matching event by Date, Country, and Event name
                    var existingEvent = existingEvents.FirstOrDefault(e =>
                        e.Date == evt.Date &&
                        e.Country == evt.Country &&
                        e.Event == evt.Event);

                    if (existingEvent != null)
                    {
                        // Update existing event
                        existingEvent.Currency = evt.Currency;
                        existingEvent.Previous = evt.Previous;
                        existingEvent.Estimate = evt.Estimate;
                        existingEvent.Actual = evt.Actual;
                        existingEvent.Change = evt.Change;
                        existingEvent.Impact = evt.Impact;
                        existingEvent.ChangePercentage = evt.ChangePercentage;
                        existingEvent.Unit = evt.Unit;
                        existingEvent.UpdatedAt = now;

                        await UpdateAsync(existingEvent);
                    }
                    else
                    {
                        // Insert new event
                        evt.CreatedAt = now;
                        evt.UpdatedAt = now;
                        await AddAsync(evt);
                    }
                }

                Console.WriteLine($"Successfully upserted {events.Count} economic calendar events.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error upserting economic calendar events: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Retrieves all economic calendar events from the database, ordered by date descending
        /// </summary>
        public async Task<List<EconomicCalendar>> GetAllEventsAsync()
        {
            var specification = new GetAllEconomicCalendarsSpecification();
            var events = await GetAsync(specification);
            Console.WriteLine($"Retrieved {events.Count} economic calendar events.");
            return events.ToList();
        }
    }
}
