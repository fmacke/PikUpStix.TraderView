using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace PikUpStix.TraderView.Services
{
    public class EquitySummaryService : IEquitySummaryService
    {
        private readonly IEquitySummaryRepository _equitySummaryRepository;

        public EquitySummaryService(IEquitySummaryRepository equitySummaryRepository)
        {
            _equitySummaryRepository = equitySummaryRepository;
        }
        public async Task<List<EquitySummary>> GetAllAsync()
        {
            var result = await _equitySummaryRepository.GetAllAsync();
            return result.ToList();
        }
        public async Task<EquitySummary?> GetByIdAsync(int id)
        {
            return await _equitySummaryRepository.GetByIdAsync(id);
        }
        public async Task<EquitySummary?> GetByAccountAndDateAsync(string accountId, DateTime reportDate)
        {
            return await _equitySummaryRepository.GetByAccountAndDateAsync(accountId, reportDate);
        }
        public async Task<List<EquitySummary>> GetByAccountIdAsync(string accountId)
        {
            var result = await _equitySummaryRepository.GetByAccountIdAsync(accountId);
            return result.ToList();
        }
        public async Task<List<EquitySummary>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var result = await _equitySummaryRepository.GetByDateRangeAsync(startDate, endDate);
            return result.ToList();
        }
        public async Task<int> CreateAsync(EquitySummary equitySummary)
        {
            var entity = await _equitySummaryRepository.AddAsync(equitySummary);
            return entity.Id;
        }
        public async Task UpdateAsync(EquitySummary equitySummary)
        {
            await _equitySummaryRepository.UpdateAsync(equitySummary);
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _equitySummaryRepository.GetByIdAsync(id);
            if (entity == null)
                return false;

            await _equitySummaryRepository.DeleteAsync(entity);
            return true;
        }
        async Task IEquitySummaryService.UpsertEquitySummariesAsync(List<EquitySummary> equitySummaries)
        {
            await _equitySummaryRepository.UpsertEquitySummariesAsync(equitySummaries);
        }
    }
}
