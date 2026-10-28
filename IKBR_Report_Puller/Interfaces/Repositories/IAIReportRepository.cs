using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Repositories
{
    public interface IAIReportRepository : IRepository<AIReport>
    {
        Task<int> CreateAsync(int reportInstructionId, string reportDetail);
        Task<bool> UpdateAsync(int id, int reportInstructionId, string reportDetail);
        Task<bool> DeleteAsync(int id);
    }
}
