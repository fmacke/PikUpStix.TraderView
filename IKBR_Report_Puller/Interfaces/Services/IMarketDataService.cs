using TraderView.Domain.Entities;
using TraderView.Domain.Entities.FMP;

namespace TraderView.Application.Interfaces.Services
{
    /// <summary>
    /// Service for retrieving and storing economic calendar data
    /// </summary>
    public interface IMarketDataService
    {
        string SourceName { get; }
        Task<List<EconomicCalendar>> FetchAndSaveEconomicCalendarAsync(DateTime fromDate, DateTime toDate);
        Task FetchAndSaveChartData(List<HistoricalTrade> trades);
        Task FetchAndSaveChartData(List<string> symbols, int lookBackDays);
        Task FetchLatestPrices(List<Position> positions);
        Task<IReadOnlyList<FmpQuarterlyIncomeStatementDto>> GetQuarterlyIncomeStatementsAsync(string symbol, int limit = 8);
        Task<CanSlimCurrentQuarterMetric?> EvaluateCurrentQuarterEpsAsync(string symbol, decimal minEpsGrowth = 25m, decimal minRevenueGrowth = 20m);
        Task<CanSlimAnnualMetric?> EvaluateAnnualEpsAsync(string symbol, decimal minCagr = 25m, decimal minRoe = 17m);
        Task<IReadOnlyList<CanSlimCandidate>> RunScreenerAsync(CanSlimScreenerCriteria criteria);
        Task<IReadOnlyList<CanSlimCandidate>> GetLatestScreenerResults();
        Task<IReadOnlyList<FmpAnnualIncomeStatementDto>> GetAnnualIncomeStatementsAsync(string symbol, int limit = 5);
        Task<IReadOnlyList<FmpKeyMetricsDto>> GetKeyMetricsTtmAsync(string symbol);
        Task<decimal> GetExchangeRate(string baseCurrency, string quoteCurrency);
    }
}
