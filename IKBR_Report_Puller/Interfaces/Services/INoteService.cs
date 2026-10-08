using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    /// <summary>
    /// Service interface for Note operations
    /// </summary>
    public interface INoteService
    {
        Task<IReadOnlyList<Note>> GetAllAsync();
        Task<Note?> GetByIdAsync(int id);
        Task<IReadOnlyList<Note>> GetByPositionIdAsync(int positionId);
        Task<IReadOnlyList<Note>> GetByTradeExecutionIdAsync(int tradeExecutionId);
        Task<IReadOnlyList<Note>> GetByTradeTypeIdAsync(int tradeTypeId);
        Task<IReadOnlyList<Note>> GetJournalEntriesAsync();
        Task<int> CreateAsync(int? positionId, int? tradeExecutionId, string comment, int? tradeTypeId, int? errorTypeId, int? exitTypeId, decimal? time);
        Task<bool> UpdateAsync(int id, int? positionId, int? tradeExecutionId, string comment, int? tradeTypeId, int? errorTypeId, int? exitTypeId, decimal? time);
        Task<bool> DeleteAsync(int id);
        Task<IReadOnlyList<Note>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
