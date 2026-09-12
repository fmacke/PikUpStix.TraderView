using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.EquitySummaries.Query.GetBy;

/// <summary>
/// Query to get all equity summaries for a date range
/// </summary>
public class GetEquitySummariesByDateRangeQuery
{
    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public GetEquitySummariesByDateRangeQuery(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}

/// <summary>
/// Handler for GetEquitySummariesByDateRangeQuery
/// </summary>
public class GetEquitySummariesByDateRangeQueryHandler
{
    private readonly IEquitySummaryService _equitySummaryService;

    public GetEquitySummariesByDateRangeQueryHandler(IEquitySummaryService equitySummaryService)
    {
        _equitySummaryService = equitySummaryService;
    }

    /// <summary>
    /// Handles the get equity summaries by date range query
    /// </summary>
    /// <param name="query">The query with start and end dates</param>
    /// <returns>List of equity summaries within the date range</returns>
    public async Task<List<EquitySummary>> Handle(GetEquitySummariesByDateRangeQuery query)
    {
        return await _equitySummaryService.GetByDateRangeAsync(query.StartDate, query.EndDate);
    }
}
