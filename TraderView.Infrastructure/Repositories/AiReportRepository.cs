using TraderView.Application.Interfaces.Repositories;
using TraderView.Domain.Entities;
using TraderView.Infrastructure.DbContexts;

namespace TraderView.Infrastructure.Repositories
{
    public class AiReportRepository : EfBaseRepository<AIReport>, IAIReportRepository
    {
        public AiReportRepository(AppDbContext db) : base(db)
        {
        }

        public async Task<int> CreateAsync(int reportInstructionId, string reportDetail)
        {
            var entity = new AIReport
            {
                ReportInstructionId = reportInstructionId,
                ReportDetail = reportDetail
            };

            var created = await AddAsync(entity);
            return created.Id;
        }

        public async Task<bool> UpdateAsync(int id, int reportInstructionId, string reportDetail)
        {
            var existing = await _db.Set<AIReport>().FindAsync(id);
            if (existing == null)
            {
                return false;
            }

            existing.ReportInstructionId = reportInstructionId;
            existing.ReportDetail = reportDetail;

            await UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _db.Set<AIReport>().FindAsync(id);
            if (existing == null)
            {
                return false;
            }

            await DeleteAsync(existing);
            return true;
        }
    }
}
