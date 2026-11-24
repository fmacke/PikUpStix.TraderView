using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface ICompanyChecklistReportQueryService
    {
        Task<CompanyChecklistReport?> GetLatestBySymbolAsync(string symbol);
        Task<IReadOnlyList<CompanyChecklistReport>> GetBySymbolAsync(string symbol);
        Task<IReadOnlyList<CompanyChecklistReport>> GetByDateRangeAsync(DateTime startDateUtc, DateTime endDateUtc);
        Task<CompanyChecklistReport?> GetByIdAsync(int id);
    }
}
