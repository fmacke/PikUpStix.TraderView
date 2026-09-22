using Microsoft.EntityFrameworkCore;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Domain.Entities;
using TraderView.Infrastructure.DbContexts;

namespace TraderView.Infrastructure.Repositories
{
    public class PositionRepository : EfBaseRepository<Position>, IPositionRepository
    {
        private readonly AppDbContext _context;

        public PositionRepository(AppDbContext db) : base(db)
        {
            _context = db;
        }

        /// <summary>
        /// Gets an open position by symbol and instrument ID.
        /// </summary>
        async Task<Position?> IPositionRepository.GetOpenPositionAsync(string symbol, int instrumentId)
        {
            _ = symbol;

            return await _context.Set<Position>()
                .AsNoTracking()
                .Include(p => p.Instrument)
                .Include(p => p.TradeExecutions)
                .Where(p => p.InstrumentId == instrumentId && p.Status == "Open")
                .OrderByDescending(p => p.OpenDate)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Creates a new position and returns its ID.
        /// </summary>
        async Task<int> IPositionRepository.CreatePositionAsync(int instrumentId, string symbol, DateTime openDate, decimal openPrice)
        {
            _ = symbol;

            var position = new Position
            {
                InstrumentId = instrumentId,
                OpenDate = openDate,
                Status = "Open",
                LastReportedPrice = openPrice,
                LastReportedPriceUpdated = DateTime.UtcNow
            };

            await _context.Set<Position>().AddAsync(position);
            await _context.SaveChangesAsync();

            return position.Id;
        }

        /// <summary>
        /// Gets all positions with Status = 'Open' and their associated trade executions.
        /// </summary>
        async Task<IReadOnlyList<Position>> IPositionRepository.GetOpenPositionsAsync()
        {
            return await _context.Set<Position>()
                .AsNoTracking()
                .Include(p => p.Instrument)
                .Include(p => p.TradeExecutions)
                .Where(p => p.Status == "Open")
                .OrderByDescending(p => p.OpenDate)
                .ToListAsync();
        }
    }
}