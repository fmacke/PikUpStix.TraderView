using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface IAIReportInstructionService
    {
        Task<IReadOnlyList<AIReportInstruction>> GetAllAsync();
        Task<AIReportInstruction?> GetByIdAsync(int id);
        Task<int> CreateAsync(AIReportInstruction instruction);
        Task<bool> UpdateAsync(int id, string instruction);
        Task<bool> DeleteAsync(int id);
    }
}
