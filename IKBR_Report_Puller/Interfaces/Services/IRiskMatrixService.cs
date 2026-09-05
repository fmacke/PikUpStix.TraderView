using TraderView.Domain.Entities.FMP;

namespace TraderView.Application.Interfaces.Services
{
    public interface IRiskMatrixService
    {
        Task<RiskMatrixCalculationResult> CalculateExpectedRoi(RiskMatrixCalculationRequest request);
        Task<RiskMatrixCalculationResult> CalculateExpectedRoi(decimal gainPercentage, decimal lossPercentage, decimal winRatePercentage, int numberOfTrades);
    }
}
