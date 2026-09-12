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
            return await Task.Run(() => _equitySummaryRepository.GetAll());
        }

        /// <summary>
        /// Gets an equity summary by its ID asynchronously
        /// </summary>
        public async Task<EquitySummary?> GetByIdAsync(int id)
        {
            return await Task.Run(() => _equitySummaryRepository.GetById(id));
        }

        /// <summary>
        /// Gets an equity summary by account ID and report date asynchronously
        /// </summary>
        public async Task<EquitySummary?> GetByAccountAndDateAsync(string accountId, DateTime reportDate)
        {
            return await Task.Run(() => _equitySummaryRepository.GetByAccountAndDate(accountId, reportDate));
        }

        /// <summary>
        /// Gets all equity summaries for a specific account asynchronously
        /// </summary>
        public async Task<List<EquitySummary>> GetByAccountIdAsync(string accountId)
        {
            return await Task.Run(() => _equitySummaryRepository.GetByAccountId(accountId));
        }

        /// <summary>
        /// Gets all equity summaries for a date range asynchronously
        /// </summary>
        public async Task<List<EquitySummary>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() => _equitySummaryRepository.GetByDateRange(startDate, endDate));
        }

        /// <summary>
        /// Creates a new equity summary asynchronously
        /// </summary>
        public async Task<int> CreateAsync(EquitySummary equitySummary)
        {
            return await Task.Run(() => _equitySummaryRepository.Create(equitySummary));
        }

        /// <summary>
        /// Updates an equity summary asynchronously
        /// </summary>
        public async Task UpdateAsync(EquitySummary equitySummary)
        {
            await Task.Run(() => _equitySummaryRepository.Update(equitySummary));
        }

        /// <summary>
        /// Deletes an equity summary by its ID asynchronously
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            return await Task.Run(() => _equitySummaryRepository.Delete(id));
        }
    }
}
