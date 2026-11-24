using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Repositories
{
    public interface ICompanyChecklistReportRepository : IRepository<CompanyChecklistReport>
    {
        Task<int> CreateAsync(string symbol, string companyName, CompanyChecklistReport report);
        Task<CompanyChecklistReport?> GetLatestBySymbolAsync(string symbol);
        Task<IReadOnlyList<CompanyChecklistReport>> GetBySymbolAsync(string symbol);
        Task<IReadOnlyList<CompanyChecklistReport>> GetByDateRangeAsync(DateTime startDateUtc, DateTime endDateUtc);
    }
}
