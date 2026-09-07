using AutoMapper;
using TraderView.Application.Models.ResultBasedForecasting;
using TraderView.Domain.Entities.FMP;

namespace traderview.Server.DTOs.Mappers
{
    public class RiskMatrixCalculationResultProfile : Profile
    {
        public RiskMatrixCalculationResultProfile()
        {
            CreateMap<RiskMatrixCalculationResult, RiskMatrixCalculationResultDto>();
        }
    }
    public class ResultBasedAssumptionForecastResultsProfile : Profile
    {
        public ResultBasedAssumptionForecastResultsProfile()
        {
            CreateMap<ResultBasedAssumptionForecastResults, ResultBasedAssumptionForecastResultsDto>();
        }
    }
}
