using TraderView.Application.Interfaces.Persistence;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Domain.Entities;
using TraderView.Infrastructure.DbContexts;

namespace TraderView.Infrastructure.Repositories
{
    public class CompoundPositionRepository : EfBaseRepository<CompoundPosition>, ICompoundPositionRepository
    {
        private readonly AppDbContext _context;

        public CompoundPositionRepository(AppDbContext db) : base(db)
        {
            _context = db;
        }
    }
}
