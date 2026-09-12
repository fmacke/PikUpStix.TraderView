using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.EquitySummaries.Query.GetBy;

/// <summary>
/// Query to get all equity summaries for a specific account
/// </summary>
public class GetEquitySummariesByAccountIdQuery
{
    public string AccountId { get; set; } = null!;

    public GetEquitySummariesByAccountIdQuery(string accountId)
    {
        AccountId = accountId;
    }
}

/// <summary>
/// Handler for GetEquitySummariesByAccountIdQuery
/// </summary>
public class GetEquitySummariesByAccountIdQueryHandler
{
    private readonly IEquitySummaryService _equitySummaryService;

    public GetEquitySummariesByAccountIdQueryHandler(IEquitySummaryService equitySummaryService)
    {
        _equitySummaryService = equitySummaryService;
    }

    /// <summary>
    /// Handles the get equity summaries by account ID query
    /// </summary>
    /// <param name="query">The query with the account ID</param>
    /// <returns>List of equity summaries for the account</returns>
    public async Task<List<EquitySummary>> Handle(GetEquitySummariesByAccountIdQuery query)
    {
        return await _equitySummaryService.GetByAccountIdAsync(query.AccountId);
    }
}
