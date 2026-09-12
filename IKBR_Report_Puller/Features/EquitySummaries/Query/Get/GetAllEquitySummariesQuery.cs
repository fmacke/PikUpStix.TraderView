using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.EquitySummaries.Query.Get;

/// <summary>
/// Query to get all equity summaries
/// </summary>
public class GetAllEquitySummariesQuery
{
}

/// <summary>
/// Handler for GetAllEquitySummariesQuery
/// </summary>
public class GetAllEquitySummariesQueryHandler
{
    private readonly IEquitySummaryService _equitySummaryService;

    public GetAllEquitySummariesQueryHandler(IEquitySummaryService equitySummaryService)
    {
        _equitySummaryService = equitySummaryService;
    }

    /// <summary>
    /// Handles the get all equity summaries query
    /// </summary>
    /// <returns>List of all equity summaries</returns>
    public async Task<List<EquitySummary>> Handle()
    {
        return await _equitySummaryService.GetAllAsync();
    }
}
