using TraderView.Domain.Entities.FMP;

namespace TraderView.Application.Interfaces.Services
{
    /// <summary>
    /// Service for running company screening and retrieving company financial data for analysis
    /// </summary>
    public interface ICompanyScreeningService
    {
        Task<IReadOnlyList<CanSlimCandidate>> RunScreenerAsync(CanSlimScreenerCriteria criteria);
        Task<IReadOnlyList<CanSlimCandidate>> GetLatestScreenerResults();
        Task<IReadOnlyList<FmpQuarterlyIncomeStatementDto>> GetQuarterlyIncomeStatementsAsync(string symbol, int limit = 8);
        Task<IReadOnlyList<FmpAnnualIncomeStatementDto>> GetAnnualIncomeStatementsAsync(string symbol, int limit = 5);
        Task<IReadOnlyList<FmpKeyMetricsDto>> GetKeyMetricsTtmAsync(string symbol);
        Task<CanSlimCurrentQuarterMetric?> EvaluateCurrentQuarterEpsAsync(string symbol, decimal minEpsGrowth = 25m, decimal minRevenueGrowth = 20m);
        Task<CanSlimAnnualMetric?> EvaluateAnnualEpsAsync(string symbol, decimal minCagr = 25m, decimal minRoe = 17m);
    }
}
