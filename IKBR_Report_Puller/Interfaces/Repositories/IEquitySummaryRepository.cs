using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Repositories;

public interface IEquitySummaryRepository
{
    /// <summary>
    /// Gets all equity summaries
    /// </summary>
    /// <returns>List of all equity summaries</returns>
    List<EquitySummary> GetAll();

    /// <summary>
    /// Gets an equity summary by its ID
    /// </summary>
    /// <param name="id">The equity summary ID</param>
    /// <returns>The equity summary or null if not found</returns>
    EquitySummary? GetById(int id);

    /// <summary>
    /// Gets an equity summary by account ID and report date
    /// </summary>
    /// <param name="accountId">The account ID</param>
    /// <param name="reportDate">The report date</param>
    /// <returns>The equity summary or null if not found</returns>
    EquitySummary? GetByAccountAndDate(string accountId, DateTime reportDate);

    /// <summary>
    /// Gets all equity summaries for a specific account
    /// </summary>
    /// <param name="accountId">The account ID</param>
    /// <returns>List of equity summaries for the account</returns>
    List<EquitySummary> GetByAccountId(string accountId);

    /// <summary>
    /// Gets all equity summaries for a date range
    /// </summary>
    /// <param name="startDate">The start date (inclusive)</param>
    /// <param name="endDate">The end date (inclusive)</param>
    /// <returns>List of equity summaries within the date range</returns>
    List<EquitySummary> GetByDateRange(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Creates a new equity summary and returns its ID
    /// </summary>
    /// <param name="equitySummary">The equity summary to create</param>
    /// <returns>The ID of the newly created equity summary</returns>
    int Create(EquitySummary equitySummary);

    /// <summary>
    /// Updates an existing equity summary
    /// </summary>
    /// <param name="equitySummary">The equity summary to update</param>
    void Update(EquitySummary equitySummary);

    /// <summary>
    /// Deletes an equity summary by its ID
    /// </summary>
    /// <param name="id">The equity summary ID to delete</param>
    /// <returns>True if deleted successfully, false if not found</returns>
    bool Delete(int id);
}
