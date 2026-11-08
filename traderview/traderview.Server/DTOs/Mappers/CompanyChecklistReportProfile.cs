using AutoMapper;
using TraderView.Application.Models.Reports;

namespace traderview.Server.DTOs.Mappers
{
    public class CompanyChecklistReportProfile : Profile
    {
        public CompanyChecklistReportProfile()
        {
            CreateMap<CompanyChecklistReport, CompanyChecklistReportDto>();
        }
    }
}
