using TraderView.Application.Models.ResultBasedForecasting;
using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface IDesiredPerformanceForecastService
    {
        DesiredPerformanceInputs GetInputsFromTradingHistory(decimal portfolioSize, decimal positionSizePercent, decimal desiredReturnPercent, List<TradeExecution> tradeExecutions);
        DesiredPerformanceResults CalculateForecast(DesiredPerformanceInputs inputs);
    }
}
