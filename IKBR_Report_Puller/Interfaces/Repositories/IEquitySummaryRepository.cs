using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for EquitySummary entities
/// </summary>
public interface IEquitySummaryRepository : IRepository<EquitySummary>
{
    /// <summary>
    /// Gets an equity summary by account ID and report date
    /// </summary>
    /// <param name="accountId">The account ID</param>
    /// <param name="reportDate">The report date</param>
    /// <returns>The equity summary or null if not found</returns>
    Task<EquitySummary?> GetByAccountAndDateAsync(string accountId, DateTime reportDate);

    /// <summary>
    /// Gets all equity summaries for a specific account
    /// </summary>
    /// <param name="accountId">The account ID</param>
    /// <returns>List of equity summaries for the account</returns>
    Task<IReadOnlyList<EquitySummary>> GetByAccountIdAsync(string accountId);

    /// <summary>
    /// Gets all equity summaries for a date range
    /// </summary>
    /// <param name="startDate">The start date (inclusive)</param>
    /// <param name="endDate">The end date (inclusive)</param>
    /// <returns>List of equity summaries within the date range</returns>
    Task<IReadOnlyList<EquitySummary>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}
