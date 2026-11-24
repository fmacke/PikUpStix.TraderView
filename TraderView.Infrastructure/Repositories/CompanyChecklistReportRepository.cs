using Microsoft.EntityFrameworkCore;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Domain.Entities;
using TraderView.Infrastructure.DbContexts;

namespace TraderView.Infrastructure.Repositories
{
    public class CompanyChecklistReportRepository : EfBaseRepository<CompanyChecklistReport>, ICompanyChecklistReportRepository
    {
        public CompanyChecklistReportRepository(AppDbContext db) : base(db)
        {
        }

        public async Task<int> CreateAsync(string symbol, string companyName, CompanyChecklistReport report)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentException("Symbol cannot be null or whitespace.", nameof(symbol));

            report.Symbol = symbol.Trim().ToUpperInvariant();
            report.CompanyName = companyName;
            report.CreatedAtUtc = DateTime.UtcNow;

            await _db.CompanyChecklistReports.AddAsync(report);
            await _db.SaveChangesAsync();

            return report.Id;
        }

        public async Task<CompanyChecklistReport?> GetLatestBySymbolAsync(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return null;

            var normalizedSymbol = symbol.Trim().ToUpperInvariant();
            return await _db.CompanyChecklistReports
                .Where(r => r.Symbol == normalizedSymbol)
                .OrderByDescending(r => r.CreatedAtUtc)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<CompanyChecklistReport>> GetBySymbolAsync(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return new List<CompanyChecklistReport>();

            var normalizedSymbol = symbol.Trim().ToUpperInvariant();
            return await _db.CompanyChecklistReports
                .Where(r => r.Symbol == normalizedSymbol)
                .OrderByDescending(r => r.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<CompanyChecklistReport>> GetByDateRangeAsync(DateTime startDateUtc, DateTime endDateUtc)
        {
            return await _db.CompanyChecklistReports
                .Where(r => r.CreatedAtUtc >= startDateUtc && r.CreatedAtUtc <= endDateUtc)
                .OrderByDescending(r => r.CreatedAtUtc)
                .ToListAsync();
        }
    }
}
