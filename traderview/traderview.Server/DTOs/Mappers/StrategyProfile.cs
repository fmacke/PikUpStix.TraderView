using AutoMapper;
using TraderView.Domain.Entities;

namespace traderview.Server.DTOs.Mappers
{
    public class StrategyProfile : Profile
    {
        public StrategyProfile()
        {
            CreateMap<Strategy, StrategyDto>();
        }
    }
}
