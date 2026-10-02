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

        public async Task<Strategy?> GetByListItemId(int listItemId)
        {
            return await Task.Run(() => _db.Set<Strategy>().FirstOrDefault(s => s.ListItemId == listItemId));
        }
    }
}
