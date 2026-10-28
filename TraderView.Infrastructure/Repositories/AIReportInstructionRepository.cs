using TraderView.Application.Interfaces.Repositories;
using TraderView.Domain.Entities;
using TraderView.Domain.Entities.FMP;
using TraderView.Infrastructure.DbContexts;

namespace TraderView.Infrastructure.Repositories
{
    public class AIReportInstructionRepository : EfBaseRepository<AIReportInstruction>, IAIReportInstructionRepository
    {
        public AIReportInstructionRepository(AppDbContext db) : base(db)
        {
        }
    }
}
