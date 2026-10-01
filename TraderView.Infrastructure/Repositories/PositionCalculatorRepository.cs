using TraderView.Application.Interfaces.Persistence;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Domain.Entities;
using TraderView.Infrastructure.DbContexts;

namespace TraderView.Infrastructure.Repositories
{
    public class PositionCalculatorRepository : EfBaseRepository<PositionCalculator>, IPositionCalculatorRepository
    {
        private readonly AppDbContext _context;

        public PositionCalculatorRepository(AppDbContext db) : base(db)
        {
            _context = db;
        }
    }
}
