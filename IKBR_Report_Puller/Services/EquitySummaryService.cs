using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace PikUpStix.TraderView.Services
{
    /// <summary>
    /// Service for EquitySummary operations
    /// </summary>
    public class EquitySummaryService : IEquitySummaryService
    {
        private readonly IEquitySummaryRepository _equitySummaryRepository;

        public EquitySummaryService(IEquitySummaryRepository equitySummaryRepository)
        {
            _equitySummaryRepository = equitySummaryRepository;
        }

        /// <summary>
        /// Gets all equity summaries asynchronously
        /// </summary>
        public async Task<List<EquitySummary>> GetAllAsync()
        {
            var result = await _equitySummaryRepository.GetAllAsync();
            return result.ToList();
        }

        /// <summary>
        /// Gets an equity summary by its ID asynchronously
        /// </summary>
        public async Task<EquitySummary?> GetByIdAsync(int id)
        {
            return await _equitySummaryRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Gets an equity summary by account ID and report date asynchronously
        /// </summary>
        public async Task<EquitySummary?> GetByAccountAndDateAsync(string accountId, DateTime reportDate)
        {
            return await _equitySummaryRepository.GetByAccountAndDateAsync(accountId, reportDate);
        }

        /// <summary>
        /// Gets all equity summaries for a specific account asynchronously
        /// </summary>
        public async Task<List<EquitySummary>> GetByAccountIdAsync(string accountId)
        {
            var result = await _equitySummaryRepository.GetByAccountIdAsync(accountId);
            return result.ToList();
        }

        /// <summary>
        /// Gets all equity summaries for a date range asynchronously
        /// </summary>
        public async Task<List<EquitySummary>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var result = await _equitySummaryRepository.GetByDateRangeAsync(startDate, endDate);
            return result.ToList();
        }

        /// <summary>
        /// Creates a new equity summary asynchronously
        /// </summary>
        public async Task<int> CreateAsync(EquitySummary equitySummary)
        {
            var entity = await _equitySummaryRepository.AddAsync(equitySummary);
            return entity.Id;
        }

        /// <summary>
        /// Updates an equity summary asynchronously
        /// </summary>
        public async Task UpdateAsync(EquitySummary equitySummary)
        {
            await _equitySummaryRepository.UpdateAsync(equitySummary);
        }

        /// <summary>
        /// Deletes an equity summary by its ID asynchronously
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _equitySummaryRepository.GetByIdAsync(id);
            if (entity == null)
                return false;

            await _equitySummaryRepository.DeleteAsync(entity);
            return true;
        }
    }
}
