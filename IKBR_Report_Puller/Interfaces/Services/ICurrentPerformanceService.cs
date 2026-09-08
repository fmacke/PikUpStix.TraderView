using TraderView.Domain.Entities.FMP;

namespace TraderView.Application.Interfaces.Services
{
    public interface ICurrentPerformanceService
    {
        Task<CurrentPerformanceResult> CalculateExpectedRoi(RiskMatrixCalculationRequest request);
        Task<CurrentPerformanceResult> CalculateExpectedRoi(decimal gainPercentage, decimal lossPercentage, decimal winRatePercentage, int numberOfTrades);
    }
}
