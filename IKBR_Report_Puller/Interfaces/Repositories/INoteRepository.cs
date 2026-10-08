using System.Threading.Tasks;
using TraderView.Domain.Entities;
using TraderView.Application.Interfaces.Persistence;

namespace TraderView.Application.Interfaces.Repositories
{
    public interface INoteRepository : IRepository<Note>
    {
        Task<IReadOnlyList<Note>> GetByPositionIdAsync(int positionId);
        Task<IReadOnlyList<Note>> GetByTradeExecutionIdAsync(int tradeExecutionId);
        Task<IReadOnlyList<Note>> GetByTradeTypeIdAsync(int tradeTypeId);
        // Get notes that are marked as Journal entries (via ListItems with Category='JournalEntry')
        Task<IReadOnlyList<Note>> GetJournalEntriesAsync();
        Task<int> InsertAsync(int? positionId, int? tradeExecutionId, string comment, DateTime entryDate, int? tradeTypeId, int? errorTypeId, int? exitTypeId, decimal? time);
        Task<bool> UpdateAsync(int id, int? positionId, int? tradeExecutionId, string comment, DateTime updatedAt, int? tradeTypeId, int? errorTypeId, int? exitTypeId, decimal? time);
        Task<IReadOnlyList<Note>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
