using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services;

public interface IEquitySummaryService
{
    Task<List<EquitySummary>> GetAllAsync();
    Task<EquitySummary?> GetByIdAsync(int id);
    Task<EquitySummary?> GetByAccountAndDateAsync(string accountId, DateTime reportDate);
    Task<List<EquitySummary>> GetByAccountIdAsync(string accountId);
    Task<List<EquitySummary>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<int> CreateAsync(EquitySummary equitySummary);
    Task UpdateAsync(EquitySummary equitySummary);
    Task<bool> DeleteAsync(int id);
    Task UpsertEquitySummariesAsync(List<EquitySummary> equitySummaries);
}
