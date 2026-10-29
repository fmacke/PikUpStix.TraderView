using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace PikUpStix.TraderView.Services
{
    public class AIReportService : IAIReportService
    {
        private readonly IAIReportRepository _aiReportRepository;

        public AIReportService(IAIReportRepository aiReportRepository)
        {
            _aiReportRepository = aiReportRepository;
        }

        public async Task<IReadOnlyList<AIReport>> GetAllAsync()
        {
            return await _aiReportRepository.GetAllAsync();
        }

        public async Task<AIReport?> GetByIdAsync(int id)
        {
            return await _aiReportRepository.GetByIdAsync(id);
        }

        public async Task<int> CreateAsync(int reportInstructionId, string reportDetail)
        {
            return await _aiReportRepository.CreateAsync(reportInstructionId, reportDetail, DateTime.Now);
        }

        public async Task<bool> UpdateAsync(int id, int reportInstructionId, string reportDetail)
        {
            return await _aiReportRepository.UpdateAsync(id, reportInstructionId, reportDetail);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _aiReportRepository.DeleteAsync(id);
        }
    }
}
