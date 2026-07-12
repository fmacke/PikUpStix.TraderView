using IKBR_Report_Puller.Domain;
using PikUpStix.TraderView.Interfaces;

namespace traderview.Server.Services
{
    /// <summary>
    /// Service for Note operations
    /// </summary>
    public class NoteService : INoteService
    {
        private readonly INoteRepository _noteRepository;
        private readonly ILogger<NoteService> _logger;

        public NoteService(
            INoteRepository noteRepository,
            ILogger<NoteService> logger)
        {
            _noteRepository = noteRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets all notes asynchronously
        /// </summary>
        public async Task<List<Note>> GetAllAsync()
        {
            try
            {
                return await Task.Run(() => _noteRepository.GetAll());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all notes");
                throw;
            }
        }

        /// <summary>
        /// Gets a note by its ID asynchronously
        /// </summary>
        public async Task<Note?> GetByIdAsync(int id)
        {
            try
            {
                return await Task.Run(() => _noteRepository.GetById(id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching note with ID {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Gets all notes for a specific position asynchronously
        /// </summary>
        public async Task<List<Note>> GetByPositionIdAsync(int positionId)
        {
            try
            {
                return await Task.Run(() => _noteRepository.GetByPositionId(positionId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notes for position {PositionId}", positionId);
                throw;
            }
        }

        /// <summary>
        /// Gets all notes for a specific trade execution asynchronously
        /// </summary>
        public async Task<List<Note>> GetByTradeExecutionIdAsync(int tradeExecutionId)
        {
            try
            {
                return await Task.Run(() => _noteRepository.GetByTradeExecutionId(tradeExecutionId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notes for trade execution {TradeExecutionId}", tradeExecutionId);
                throw;
            }
        }

        /// <summary>
        /// Gets all notes for a specific trade type asynchronously
        /// </summary>
        public async Task<List<Note>> GetByTradeTypeIdAsync(int tradeTypeId)
        {
            try
            {
                return await Task.Run(() => _noteRepository.GetByTradeTypeId(tradeTypeId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notes for trade type {TradeTypeId}", tradeTypeId);
                throw;
            }
        }

        /// <summary>
        /// Creates a new note asynchronously
        /// </summary>
        public async Task<int> CreateAsync(int positionId, int? tradeExecutionId, string comment, DateTime entryDate, int tradeTypeId)
        {
            try
            {
                return await Task.Run(() => _noteRepository.Insert(positionId, tradeExecutionId, comment, entryDate, tradeTypeId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating note for position {PositionId}", positionId);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing note asynchronously
        /// </summary>
        public async Task<bool> UpdateAsync(int id, int positionId, int? tradeExecutionId, string comment, DateTime entryDate, int tradeTypeId)
        {
            try
            {
                return await Task.Run(() => _noteRepository.Update(id, positionId, tradeExecutionId, comment, entryDate, tradeTypeId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating note with ID {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Deletes a note by its ID asynchronously
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                return await Task.Run(() => _noteRepository.Delete(id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting note with ID {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Gets notes within a date range asynchronously
        /// </summary>
        public async Task<List<Note>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await Task.Run(() => _noteRepository.GetByDateRange(startDate, endDate));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notes between {StartDate} and {EndDate}", startDate, endDate);
                throw;
            }
        }
    }
}
