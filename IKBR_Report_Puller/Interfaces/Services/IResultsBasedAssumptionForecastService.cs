using TraderView.Application.Models.ResultBasedForecasting;

namespace TraderView.Application.Interfaces.Services
{
    public interface IResultsBasedAssumptionForecastService
    {
        ResultBasedAssumptionForecastResults CalculateForecast(ResultBasedAssumptionForecastInputs inputs);
    }
}
