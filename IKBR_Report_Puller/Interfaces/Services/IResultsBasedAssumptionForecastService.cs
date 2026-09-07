using TraderView.Application.Models.ResultBasedForecasting;
using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface IResultsBasedAssumptionForecastService
    {
        ResultBasedAssumptionForecastInputs GetInputsFromTradingHistory(decimal portfolioSize, decimal positionSizePercent, decimal desiredReturnPercent, List<TradeExecution> tradeExecutions);
        ResultBasedAssumptionForecastResults CalculateForecast(ResultBasedAssumptionForecastInputs inputs);
    }
}
