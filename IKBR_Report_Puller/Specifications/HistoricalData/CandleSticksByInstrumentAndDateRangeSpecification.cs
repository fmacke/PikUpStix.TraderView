using System.Linq.Expressions;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.HistoricalData
{
    /// <summary>
    /// Specification for retrieving candlestick data for a given instrument within a date range
    /// </summary>
    public class CandleSticksByInstrumentAndDateRangeSpecification : BaseSpecification<HistoricalDatum>
    {
        public CandleSticksByInstrumentAndDateRangeSpecification(int instrumentId, DateTime startDate, DateTime endDate)
        {
            Criteria = hd => hd.InstrumentId == instrumentId 
                && hd.Date >= startDate 
                && hd.Date <= endDate;

            ApplyOrdering(hd => hd.Date, isDescending: false);
        }
    }
}
