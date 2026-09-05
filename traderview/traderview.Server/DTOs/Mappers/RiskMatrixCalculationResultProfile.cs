using AutoMapper;
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
}
