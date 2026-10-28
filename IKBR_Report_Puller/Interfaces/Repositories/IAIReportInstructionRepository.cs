using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Repositories
{
    public interface IAIReportInstructionRepository : IRepository<AIReportInstruction>
    {
        Task<AIReportInstruction?> GetByIdAsync(int id);
    }
}