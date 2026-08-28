using TraderView.Domain.Entities;
using TraderView.Domain.Entities.FMP;

namespace TraderView.Application.Interfaces.Services
{
    /// <summary>
    /// Service for retrieving and storing economic calendar data
    /// </summary>
    public interface IMarketDataService
    {
        /// <summary>
        /// Retrieves economic calendar events for a date range, saves to file and database
        /// </summary>
        /// <param name="fromDate">Start date for calendar events</param>
        /// <param name="toDate">End date for calendar events</param>
        /// <returns>List of economic calendar events</returns>
        Task<List<EconomicCalendar>> FetchAndSaveEconomicCalendarAsync(DateTime fromDate, DateTime toDate);
        Task FetchAndSaveChartData(List<HistoricalTrade> trades);
        Task FetchAndSaveChartData(List<string> symbols, int lookBackDays);
        Task FetchLatestPrices(List<Position> positions);
        string SourceName { get; }
        Task<IReadOnlyList<FmpQuarterlyIncomeStatementDto>> GetQuarterlyIncomeStatementsAsync(string symbol, int limit = 8);
        Task<CanSlimCurrentQuarterMetric?> EvaluateCurrentQuarterEpsAsync(string symbol, decimal minEpsGrowth = 25m, decimal minRevenueGrowth = 20m);
        Task<CanSlimAnnualMetric?> EvaluateAnnualEpsAsync(string symbol, decimal minCagr = 25m, decimal minRoe = 17m);
        Task<IReadOnlyList<CanSlimCandidate>> RunScreenerAsync(CanSlimScreenerCriteria criteria);
        Task<IReadOnlyList<FmpAnnualIncomeStatementDto>> GetAnnualIncomeStatementsAsync(string symbol, int limit = 5);
        Task<IReadOnlyList<FmpKeyMetricsDto>> GetKeyMetricsTtmAsync(string symbol);
    }
}
