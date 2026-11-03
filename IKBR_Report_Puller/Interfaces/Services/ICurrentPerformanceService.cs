using TraderView.Domain.Entities;
using TraderView.Domain.Entities.FMP;

namespace TraderView.Application.Interfaces.Services
{
    public interface ICurrentPerformanceService
    {
        Task<CurrentPerformanceResult> CalculateExpectedRoi(RiskMatrixCalculationRequest request, IReadOnlyList<Position>? positions = null);
        Task<CurrentPerformanceResult> CalculateExpectedRoi(decimal gainPercentage, decimal lossPercentage, decimal winRatePercentage, int numberOfTrades, IReadOnlyList<Position>? positions = null);
    }
}
