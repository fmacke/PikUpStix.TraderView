using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Specifications.Notes;
using TraderView.Domain.Entities;
using TraderView.Infrastructure.DbContexts;

namespace TraderView.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for Note-related database operations
    /// </summary>
    public class NoteRepository : EfBaseRepository<Note>, INoteRepository
    {
        public NoteRepository(AppDbContext db) : base(db)
        {
        }

        /// <summary>
        /// Gets all notes for a specific position
        /// </summary>
        public async Task<IReadOnlyList<Note>> GetByPositionIdAsync(int positionId)
        {
            var specification = new NoteByPositionSpecification(positionId);
            return await GetAsync(specification);
        }

        /// <summary>
        /// Gets all notes for a specific trade execution
        /// </summary>
        public async Task<IReadOnlyList<Note>> GetByTradeExecutionIdAsync(int tradeExecutionId)
        {
            var specification = new NoteByTradeExecutionSpecification(tradeExecutionId);
            return await GetAsync(specification);
        }

        /// <summary>
        /// Gets all notes for a specific trade type
        /// </summary>
        public async Task<IReadOnlyList<Note>> GetByTradeTypeIdAsync(int tradeTypeId)
        {
            var specification = new NoteByTradeTypeSpecification(tradeTypeId);
            return await GetAsync(specification);
        }

        /// <summary>
        /// Gets notes within a date range
        /// </summary>
        public async Task<IReadOnlyList<Note>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var specification = new NoteByDateRangeSpecification(startDate, endDate);
            return await GetAsync(specification);
        }
        /// <summary>
        /// Inserts a new note into the database
        /// </summary>
        public async Task<int> InsertAsync(int positionId, int? tradeExecutionId, string comment, DateTime entryDate, int? tradeTypeId, int? errorTypeId)
        {
            var note = new Note
            {
                PositionId = positionId,
                TradeExecutionId = tradeExecutionId,
                Comment = comment,
                EntryDate = entryDate,
                UpdatedAt = entryDate,
                TradeTypeId = tradeTypeId,
                ErrorTypeId = errorTypeId
            };

            var inserted = await AddAsync(note);
            return inserted.Id;
        }

        /// <summary>
        /// Updates an existing note
        /// </summary>
        public async Task<bool> UpdateAsync(int id, int positionId, int? tradeExecutionId, string comment, DateTime updatedAt, int? tradeTypeId, int? errorTypeId)
        {
            var note = await GetByIdAsync(id);
            if (note == null)
                return false;

            note.PositionId = positionId;
            note.TradeExecutionId = tradeExecutionId;
            note.Comment = comment;
            note.UpdatedAt = updatedAt;
            note.TradeTypeId = tradeTypeId;
            note.ErrorTypeId = errorTypeId;

            await UpdateAsync(note);
            return true;
        }
    }
}
