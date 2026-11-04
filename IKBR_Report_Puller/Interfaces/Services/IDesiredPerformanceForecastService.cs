using TraderView.Application.Models.ResultBasedForecasting;
using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface IDesiredPerformanceForecastService
    {
        DesiredPerformanceInputs GetInputsFromTradingHistory(decimal portfolioSize, decimal positionSizePercent, decimal desiredReturnPercent, IReadOnlyList<Position> positions);
        DesiredPerformanceResults CalculateForecast(DesiredPerformanceInputs inputs);
    }
}
