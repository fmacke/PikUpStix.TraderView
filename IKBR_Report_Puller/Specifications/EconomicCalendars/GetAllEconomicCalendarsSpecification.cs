using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.EconomicCalendars
{
    /// <summary>
    /// Specification for retrieving all economic calendar events ordered by date descending
    /// </summary>
    public class GetAllEconomicCalendarsSpecification : BaseSpecification<EconomicCalendar>
    {
        public GetAllEconomicCalendarsSpecification()
        {
            ApplyOrdering(e => e.Date, isDescending: true);
        }
    }
}
