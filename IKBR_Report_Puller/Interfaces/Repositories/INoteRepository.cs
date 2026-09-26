using System.Threading.Tasks;
using TraderView.Domain.Entities;
using TraderView.Application.Interfaces.Persistence;

namespace TraderView.Application.Interfaces.Repositories
{
    /// <summary>
    /// Async repository interface for Note-related database operations
    /// </summary>
    public interface INoteRepository : IRepository<Note>
    {
        /// <summary>
        /// Gets all notes for a specific position
        /// </summary>
        Task<IReadOnlyList<Note>> GetByPositionIdAsync(int positionId);

        /// <summary>
        /// Gets all notes for a specific trade execution
        /// </summary>
        Task<IReadOnlyList<Note>> GetByTradeExecutionIdAsync(int tradeExecutionId);

        /// <summary>
        /// Gets all notes for a specific trade type
        /// </summary>
        Task<IReadOnlyList<Note>> GetByTradeTypeIdAsync(int tradeTypeId);

        /// <summary>
        /// Inserts a new note into the database
        /// </summary>
        Task<int> InsertAsync(int positionId, int? tradeExecutionId, string comment, DateTime entryDate, int? tradeTypeId, int? errorTypeId);

        /// <summary>
        /// Updates an existing note
        /// </summary>
        Task<bool> UpdateAsync(int id, int positionId, int? tradeExecutionId, string comment, DateTime updatedAt, int? tradeTypeId, int? errorTypeId);

        /// <summary>
        /// Gets notes within a date range
        /// </summary>
        Task<IReadOnlyList<Note>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
