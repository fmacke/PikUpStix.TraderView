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
            CreateMap<TradeHistoryReport, CurrentPerformanceResultDto>()
                .ForMember(dest => dest.GainPercentage, opt => opt.MapFrom(src => Math.Round(src.GainPercentage * 100, 2)))
                .ForMember(dest => dest.LossPercentage, opt => opt.MapFrom(src => Math.Round(src.LossPercentage * 100, 2)))
                .ForMember(dest => dest.RewardToRiskRatio, opt => opt.MapFrom(src => Math.Round(src.RewardToRiskRatio * 100, 2)))
                .ForMember(dest => dest.WinRatePercentage, opt => opt.MapFrom(src => Math.Round(src.WinRatePercentage * 100, 2)))
                .ForMember(dest => dest.LossRatePercentage, opt => opt.MapFrom(src => Math.Round(src.LossRatePercentage * 100, 2)))
                .ForMember(dest => dest.SimpleRoi, opt => opt.MapFrom(src => Math.Round(src.SimpleRoi * 100, 2)))
                .ForMember(dest => dest.CompoundedRoi, opt => opt.MapFrom(src => Math.Round(src.CompoundedRoi * 100, 2)))
                .ForMember(dest => dest.ExpectedReturnPerTrade, opt => opt.MapFrom(src => Math.Round(src.ExpectedReturnPerTrade * 100, 2)))
                .ForMember(dest => dest.AverageDaysHeld, opt => opt.MapFrom(src => src.AverageDaysHeld))
                .ForMember(dest => dest.AverageDaysHeldGains, opt => opt.MapFrom(src => src.AverageDaysHeldGains))
                .ForMember(dest => dest.AverageDaysHeldLosses, opt => opt.MapFrom(src => src.AverageDaysHeldLosses));
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
