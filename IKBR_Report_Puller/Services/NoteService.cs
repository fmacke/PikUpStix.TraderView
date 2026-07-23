using IKBR_Report_Puller.Domain;
using PikUpStix.TraderView.Interfaces;

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
        public async Task<List<Note>> GetAllAsync()
        {
            return await Task.Run(() => _noteRepository.GetAll());
        }

        /// <summary>
        /// Gets a note by its ID asynchronously
        /// </summary>
        public async Task<Note?> GetByIdAsync(int id)
        {
            return await Task.Run(() => _noteRepository.GetById(id));
        }

        /// <summary>
        /// Gets all notes for a specific position asynchronously
        /// </summary>
        public async Task<List<Note>> GetByPositionIdAsync(int positionId)
        {
            return await Task.Run(() => _noteRepository.GetByPositionId(positionId));
        }

        /// <summary>
        /// Gets all notes for a specific trade execution asynchronously
        /// </summary>
        public async Task<List<Note>> GetByTradeExecutionIdAsync(int tradeExecutionId)
        {
            return await Task.Run(() => _noteRepository.GetByTradeExecutionId(tradeExecutionId));
        }

        /// <summary>
        /// Gets all notes for a specific trade type asynchronously
        /// </summary>
        public async Task<List<Note>> GetByTradeTypeIdAsync(int tradeTypeId)
        {
            return await Task.Run(() => _noteRepository.GetByTradeTypeId(tradeTypeId));
        }

        /// <summary>
        /// Creates a new note asynchronously
        /// </summary>
        public async Task<int> CreateAsync(int positionId, int? tradeExecutionId, string comment, DateTime entryDate, int tradeTypeId)
        {
            return await Task.Run(() => _noteRepository.Insert(positionId, tradeExecutionId, comment, entryDate, tradeTypeId));
        }

        /// <summary>
        /// Updates an existing note asynchronously
        /// </summary>
        public async Task<bool> UpdateAsync(int id, int positionId, int? tradeExecutionId, string comment, DateTime entryDate, int tradeTypeId)
        {
            return await Task.Run(() => _noteRepository.Update(id, positionId, tradeExecutionId, comment, entryDate, tradeTypeId));
        }

        /// <summary>
        /// Deletes a note by its ID asynchronously
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            return await Task.Run(() => _noteRepository.Delete(id));
        }

        /// <summary>
        /// Gets notes within a date range asynchronously
        /// </summary>
        public async Task<List<Note>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() => _noteRepository.GetByDateRange(startDate, endDate));
        }
    }
}
