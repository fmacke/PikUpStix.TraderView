using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface IAIReportService
    {
        Task<IReadOnlyList<AIReport>> GetAllAsync();
        Task<AIReport?> GetByIdAsync(int id);
        Task<int> CreateAsync(int reportInstructionId, string reportDetail);
        Task<bool> UpdateAsync(int id, int reportInstructionId, string reportDetail);
        Task<bool> DeleteAsync(int id);
    }
}
