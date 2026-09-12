using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.EquitySummaries.Query.GetBy;

/// <summary>
/// Query to get an equity summary by ID
/// </summary>
public class GetEquitySummaryByIdQuery
{
    public int Id { get; set; }

    public GetEquitySummaryByIdQuery(int id)
    {
        Id = id;
    }
}

/// <summary>
/// Handler for GetEquitySummaryByIdQuery
/// </summary>
public class GetEquitySummaryByIdQueryHandler
{
    private readonly IEquitySummaryService _equitySummaryService;

    public GetEquitySummaryByIdQueryHandler(IEquitySummaryService equitySummaryService)
    {
        _equitySummaryService = equitySummaryService;
    }

    /// <summary>
    /// Handles the get equity summary by ID query
    /// </summary>
    /// <param name="query">The query with the equity summary ID</param>
    /// <returns>The equity summary or null if not found</returns>
    public async Task<EquitySummary?> Handle(GetEquitySummaryByIdQuery query)
    {
        return await _equitySummaryService.GetByIdAsync(query.Id);
    }
}
