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
        private readonly IListService _listService;

        public NoteService(INoteRepository noteRepository, IListService listService)
        {
            _noteRepository = noteRepository;
            _listService = listService;
        }
        public async Task<IReadOnlyList<Note>> GetAllAsync()
        {
            return await Task.Run(() => _noteRepository.GetAllAsync());
        }
        public async Task<Note?> GetByIdAsync(int id)
        {
            return await Task.Run(() => _noteRepository.GetByIdAsync(id));
        }
        public async Task<IReadOnlyList<Note>> GetByPositionIdAsync(int positionId)
        {
            return await Task.Run(() => _noteRepository.GetByPositionIdAsync(positionId));
        }

        public async Task<IReadOnlyList<Note>> GetByTradeExecutionIdAsync(int tradeExecutionId)
        {
            return await Task.Run(() => _noteRepository.GetByTradeExecutionIdAsync(tradeExecutionId));
        }

        public async Task<IReadOnlyList<Note>> GetByTradeTypeIdAsync(int tradeTypeId)
        {
            return await Task.Run(() => _noteRepository.GetByTradeTypeIdAsync(tradeTypeId));
        }
        public async Task<int> CreateAsync(int? positionId, int? tradeExecutionId, string comment, DateTime entryDate, int? tradeTypeId, int? errorTypeId, int? exitTypeId, decimal? time)
        {
            return await Task.Run(() => _noteRepository.InsertAsync(positionId, tradeExecutionId, comment, entryDate, tradeTypeId, errorTypeId, exitTypeId, time));
        }
        public async Task<bool> UpdateAsync(int id, int? positionId, int? tradeExecutionId, string comment, DateTime updatedAt, int? tradeTypeId, int? errorTypeId, int? exitTypeId, decimal? time)
        {
            return await Task.Run(() => _noteRepository.UpdateAsync(id, positionId, tradeExecutionId, comment, updatedAt, tradeTypeId, errorTypeId, exitTypeId, time));
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var note = await _noteRepository.GetByIdAsync(id);
            if (note == null)
                return false;

            await _noteRepository.DeleteAsync(note);
            return true;
        }
        public async Task<IReadOnlyList<Note>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() => _noteRepository.GetByDateRangeAsync(startDate, endDate));
        }
        public async Task<IReadOnlyList<Note>> GetJournalEntriesAsync()
        {
            return await Task.Run(() => _noteRepository.GetJournalEntriesAsync());
        }
    }
}
