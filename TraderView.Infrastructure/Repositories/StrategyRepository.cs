using TraderView.Application.Interfaces.Repositories;
using TraderView.Domain.Entities;
using TraderView.Infrastructure.DbContexts;

namespace TraderView.Infrastructure.Repositories
{
    public class StrategyRepository : EfBaseRepository<Strategy>, IStrategyRepository
    {
        public StrategyRepository(AppDbContext db) : base(db)
        {
        }
    }
}
