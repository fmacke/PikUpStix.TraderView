using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace PikUpStix.TraderView.Services
{
    /// <summary>
    /// Service for Note operations
    /// </summary>
    public class NoteService : INoteService
    {
        private readonly INoteRepository _noteRepository;

        public NoteService(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        /// <summary>
        /// Gets all notes asynchronously
        /// </summary>
        public async Task<IReadOnlyList<Note>> GetAllAsync()
        {
            return await Task.Run(() => _noteRepository.GetAllAsync());
        }

        /// <summary>
        /// Gets a note by its ID asynchronously
        /// </summary>
        public async Task<Note?> GetByIdAsync(int id)
        {
            return await Task.Run(() => _noteRepository.GetByIdAsync(id));
        }

        /// <summary>
        /// Gets all notes for a specific position asynchronously
        /// </summary>
        public async Task<IReadOnlyList<Note>> GetByPositionIdAsync(int positionId)
        {
            return await Task.Run(() => _noteRepository.GetByPositionIdAsync(positionId));
        }

        /// <summary>
        /// Gets all notes for a specific trade execution asynchronously
        /// </summary>
        public async Task<IReadOnlyList<Note>> GetByTradeExecutionIdAsync(int tradeExecutionId)
        {
            return await Task.Run(() => _noteRepository.GetByTradeExecutionIdAsync(tradeExecutionId));
        }

        /// <summary>
        /// Gets all notes for a specific trade type asynchronously
        /// </summary>
        public async Task<IReadOnlyList<Note>> GetByTradeTypeIdAsync(int tradeTypeId)
        {
            return await Task.Run(() => _noteRepository.GetByTradeTypeIdAsync(tradeTypeId));
        }

        /// <summary>
        /// Creates a new note asynchronously
        /// </summary>
        public async Task<int> CreateAsync(int positionId, int? tradeExecutionId, string comment, DateTime entryDate, int? tradeTypeId, int? errorTypeId)
        {
            return await Task.Run(() => _noteRepository.InsertAsync(positionId, tradeExecutionId, comment, entryDate, tradeTypeId, errorTypeId));
        }

        /// <summary>
        /// Updates an existing note asynchronously
        /// </summary>
        public async Task<bool> UpdateAsync(int id, int positionId, int? tradeExecutionId, string comment, DateTime updatedAt, int? tradeTypeId, int? errorTypeId)
        {
            return await Task.Run(() => _noteRepository.UpdateAsync(id, positionId, tradeExecutionId, comment, updatedAt, tradeTypeId, errorTypeId));
        }

        /// <summary>
        /// Deletes a note by its ID asynchronously
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var note = await _noteRepository.GetByIdAsync(id);
            if (note == null)
                return false;

            await _noteRepository.DeleteAsync(note);
            return true;
        }

        /// <summary>
        /// Gets notes within a date range asynchronously
        /// </summary>
        public async Task<IReadOnlyList<Note>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() => _noteRepository.GetByDateRangeAsync(startDate, endDate));
        }
    }
}
