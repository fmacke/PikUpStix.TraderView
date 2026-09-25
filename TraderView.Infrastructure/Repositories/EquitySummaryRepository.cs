using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Specifications.EquitySummaries;
using TraderView.Domain.Entities;
using TraderView.Infrastructure.DbContexts;

namespace TraderView.Infrastructure.Repositories
{
    /// <summary>
    /// Entity Framework implementation of the EquitySummary repository
    /// </summary>
    public class EquitySummaryRepository : EfBaseRepository<EquitySummary>, IEquitySummaryRepository
    {
        public EquitySummaryRepository(AppDbContext db) : base(db)
        {
        }

        /// <summary>
        /// Gets an equity summary by account ID and report date
        /// </summary>
        public async Task<EquitySummary?> GetByAccountAndDateAsync(string accountId, DateTime reportDate)
        {
            var specification = new EquitySummaryByAccountAndDateSpecification(accountId, reportDate);
            return await GetSingleAsync(specification);
        }

        /// <summary>
        /// Gets all equity summaries for a specific account
        /// </summary>
        public async Task<IReadOnlyList<EquitySummary>> GetByAccountIdAsync(string accountId)
        {
            var specification = new EquitySummaryByAccountIdSpecification(accountId);
            return await GetAsync(specification);
        }

        /// <summary>
        /// Gets all equity summaries for a date range
        /// </summary>
        public async Task<IReadOnlyList<EquitySummary>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var specification = new EquitySummaryByDateRangeSpecification(startDate, endDate);
            return await GetAsync(specification);
        }
    }
}
