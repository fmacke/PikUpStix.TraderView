using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services.AI
{
    public interface IGoogleAIStudioService
    {
        Task<string> GenerateReportAsync(int instructionId, IReadOnlyList<Position> openPositions);
    }
}