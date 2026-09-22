using System.Threading.Tasks;
using TraderView.Domain.Entities;
using TraderView.Application.Interfaces.Persistence;

namespace TraderView.Application.Interfaces.Repositories
{
    /// <summary>
    /// Async repository interface for Note-related database operations
    /// </summary>
    public interface INoteRepository
    {
        Task<IReadOnlyList<Note>> GetAllAsync();

        Task<Note?> GetByIdAsync(int id);

        Task<IReadOnlyList<Note>> GetByPositionIdAsync(int positionId);

        Task<IReadOnlyList<Note>> GetByTradeExecutionIdAsync(int tradeExecutionId);

        Task<IReadOnlyList<Note>> GetByTradeTypeIdAsync(int tradeTypeId);

        Task<int> InsertAsync(int positionId, int? tradeExecutionId, string comment, DateTime entryDate, int? tradeTypeId, int? errorTypeId);

        Task<bool> UpdateAsync(int id, int positionId, int? tradeExecutionId, string comment, DateTime updatedAt, int? tradeTypeId, int? errorTypeId);

        Task<bool> DeleteAsync(int id);

        Task<IReadOnlyList<Note>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
