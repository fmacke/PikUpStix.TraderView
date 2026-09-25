using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    /// <summary>
    /// Service for retrieving and storing general market data (economic calendars, price data, chart data)
    /// </summary>
    public interface IMarketDataService
    {
        string SourceName { get; }

        Task<decimal> GetExchangeRate(string baseCurrency, string quoteCurrency);
        Task<List<EconomicCalendar>> FetchAndSaveEconomicCalendarAsync(DateTime fromDate, DateTime toDate);
        Task FetchAndSaveChartData(List<HistoricalTrade> trades);
        Task FetchAndSaveChartData(List<string> symbols, int lookBackDays);
        Task FetchLatestPrices(List<Position> positions);
    }
}
