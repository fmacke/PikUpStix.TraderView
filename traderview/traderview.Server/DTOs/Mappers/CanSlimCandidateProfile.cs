using AutoMapper;
using TraderView.Domain.Entities.FMP;
using traderview.Server.Dtos;

namespace traderview.Server.DTOs.Mappers
{
    public class CanSlimCandidateProfile : Profile
    {
        public CanSlimCandidateProfile()
        {
            CreateMap<CanSlimCandidate, CanSlimCandidateDto>();
        }
    }
}
