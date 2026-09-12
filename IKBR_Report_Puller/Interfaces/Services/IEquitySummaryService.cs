using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services;

public interface IEquitySummaryService
{
    /// <summary>
    /// Gets all equity summaries asynchronously
    /// </summary>
    Task<List<EquitySummary>> GetAllAsync();

    /// <summary>
    /// Gets an equity summary by its ID asynchronously
    /// </summary>
    /// <param name="id">The equity summary ID</param>
    Task<EquitySummary?> GetByIdAsync(int id);

    /// <summary>
    /// Gets an equity summary by account ID and report date asynchronously
    /// </summary>
    /// <param name="accountId">The account ID</param>
    /// <param name="reportDate">The report date</param>
    Task<EquitySummary?> GetByAccountAndDateAsync(string accountId, DateTime reportDate);

    /// <summary>
    /// Gets all equity summaries for a specific account asynchronously
    /// </summary>
    /// <param name="accountId">The account ID</param>
    Task<List<EquitySummary>> GetByAccountIdAsync(string accountId);

    /// <summary>
    /// Gets all equity summaries for a date range asynchronously
    /// </summary>
    /// <param name="startDate">The start date (inclusive)</param>
    /// <param name="endDate">The end date (inclusive)</param>
    Task<List<EquitySummary>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Creates a new equity summary asynchronously
    /// </summary>
    /// <param name="equitySummary">The equity summary to create</param>
    Task<int> CreateAsync(EquitySummary equitySummary);

    /// <summary>
    /// Updates an equity summary asynchronously
    /// </summary>
    /// <param name="equitySummary">The equity summary to update</param>
    Task UpdateAsync(EquitySummary equitySummary);

    /// <summary>
    /// Deletes an equity summary by its ID asynchronously
    /// </summary>
    /// <param name="id">The equity summary ID to delete</param>
    /// <returns>True if deleted successfully, false if not found</returns>
    Task<bool> DeleteAsync(int id);
}
