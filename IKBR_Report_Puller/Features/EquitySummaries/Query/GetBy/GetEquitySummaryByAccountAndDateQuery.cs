using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.EquitySummaries.Query.GetBy;

/// <summary>
/// Query to get an equity summary by account ID and report date
/// </summary>
public class GetEquitySummaryByAccountAndDateQuery
{
    public string AccountId { get; set; } = null!;

    public DateTime ReportDate { get; set; }

    public GetEquitySummaryByAccountAndDateQuery(string accountId, DateTime reportDate)
    {
        AccountId = accountId;
        ReportDate = reportDate;
    }
}

/// <summary>
/// Handler for GetEquitySummaryByAccountAndDateQuery
/// </summary>
public class GetEquitySummaryByAccountAndDateQueryHandler
{
    private readonly IEquitySummaryService _equitySummaryService;

    public GetEquitySummaryByAccountAndDateQueryHandler(IEquitySummaryService equitySummaryService)
    {
        _equitySummaryService = equitySummaryService;
    }

    /// <summary>
    /// Handles the get equity summary by account and date query
    /// </summary>
    /// <param name="query">The query with account ID and report date</param>
    /// <returns>The equity summary or null if not found</returns>
    public async Task<EquitySummary?> Handle(GetEquitySummaryByAccountAndDateQuery query)
    {
        return await _equitySummaryService.GetByAccountAndDateAsync(query.AccountId, query.ReportDate);
    }
}
