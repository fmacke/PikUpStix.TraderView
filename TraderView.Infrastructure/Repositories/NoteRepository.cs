using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Specifications.Notes;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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
        /// Gets notes that are flagged as Journal entries by joining to ListItems where Category = 'JournalEntry'
        /// </summary>
        public async Task<IReadOnlyList<Note>> GetJournalEntriesAsync()
        {
            return await GetAllAsync();
            try
            {
                // Use an existence subquery to find Notes whose TradeTypeId links to a ListItem
                // with Category = 'JournalEntry'. This mirrors the SQL INNER JOIN used previously
                // but avoids materializing the ListItem ids separately.
                var query = _db.Notes
                    .Where(n => _db.ListItems.Any(li => li.Id == n.TradeTypeId && li.Category == "JournalEntry"))
                    .OrderByDescending(n => n.EntryDate)
                    .AsNoTracking();

                // Log the generated SQL when debugging (uncomment if needed)
                // Console.WriteLine(query.ToQueryString());

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error getting journal entries: {ex.Message}");
                throw; // Rethrow the exception to be handled by the caller
            }

            
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
        public async Task<int> InsertAsync(int? positionId, int? tradeExecutionId, string comment, DateTime entryDate, int? tradeTypeId, int? errorTypeId, int? exitTypeId, decimal? time)
        {
            try
            {
                var note = new Note
                {
                    PositionId = positionId,
                    TradeExecutionId = tradeExecutionId,
                    Comment = comment,
                    EntryDate = entryDate,
                    UpdatedAt = entryDate,
                    Time = time,
                    TradeTypeId = tradeTypeId,
                    ErrorTypeId = errorTypeId,
                    ExitTypeId = exitTypeId
                };

                var inserted = await AddAsync(note);
                return inserted.Id;
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Error inserting note: {ex.Message}");
                throw; // Rethrow the exception to be handled by the caller
            }
        }

        /// <summary>
        /// Updates an existing note
        /// </summary>
        public async Task<bool> UpdateAsync(int id, int? positionId, int? tradeExecutionId, string comment, DateTime updatedAt, int? tradeTypeId, int? errorTypeId, int? exitTypeId, decimal? time)
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
            note.ExitTypeId = exitTypeId;
            note.Time = time;

            await UpdateAsync(note);
            return true;
        }
    }
}
