using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface IRiskMatrixService
    {
        RiskMatrixCalculationResult CalculateExpectedRoi(RiskMatrixCalculationRequest request);

        RiskMatrixCalculationResult CalculateExpectedRoi(
            decimal gainPercentage,
            decimal lossPercentage,
            decimal winRatePercentage,
            int numberOfTrades);
    }
}
