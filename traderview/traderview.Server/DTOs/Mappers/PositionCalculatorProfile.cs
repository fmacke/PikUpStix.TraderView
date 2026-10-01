using AutoMapper;
using traderview.Server.DTOs;
using TraderView.Domain.Entities;

namespace traderview.Server.DTOs.Mappers;

public class PositionCalculatorProfile : Profile
{
    public PositionCalculatorProfile()
    {
        CreateMap<PositionCalculator, PositionCalculatorDto>().ReverseMap();
        CreateMap<PositionCalculatorCreateDto, PositionCalculator>();
        CreateMap<PositionCalculatorUpdateDto, PositionCalculator>();
    }
}
