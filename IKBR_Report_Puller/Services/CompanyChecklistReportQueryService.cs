using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace TraderView.Application.Services
{
    public class CompanyChecklistReportQueryService : ICompanyChecklistReportQueryService
    {
        private readonly ICompanyChecklistReportRepository _repository;

        public CompanyChecklistReportQueryService(ICompanyChecklistReportRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<CompanyChecklistReport?> GetLatestBySymbolAsync(string symbol)
        {
            return await _repository.GetLatestBySymbolAsync(symbol);
        }

        public async Task<IReadOnlyList<CompanyChecklistReport>> GetBySymbolAsync(string symbol)
        {
            return await _repository.GetBySymbolAsync(symbol);
        }

        public async Task<IReadOnlyList<CompanyChecklistReport>> GetByDateRangeAsync(DateTime startDateUtc, DateTime endDateUtc)
        {
            return await _repository.GetByDateRangeAsync(startDateUtc, endDateUtc);
        }

        public async Task<CompanyChecklistReport?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }
}
