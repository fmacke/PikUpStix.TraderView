using AutoMapper;
using TraderView.Application.Models;
using TraderView.Application.Models.ResultBasedForecasting;
using TraderView.Domain.Entities.FMP;

namespace traderview.Server.DTOs.Mappers
{
    public class CurrentPerformanceProfile : Profile
    {
        public CurrentPerformanceProfile()
        {
            CreateMap<TradeHistoryReport, CurrentPerformanceResultDto>();
        }
    }
    public class DesiredPerformanceResultsProfile : Profile
    {
        public DesiredPerformanceResultsProfile()
        {
            CreateMap<DesiredPerformanceResults, DesiredPerformanceResultsDto>();
        }
    }
}
