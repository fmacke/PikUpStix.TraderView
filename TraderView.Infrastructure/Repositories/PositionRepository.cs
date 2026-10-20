using Microsoft.EntityFrameworkCore;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Domain.Entities;
using TraderView.Infrastructure.DbContexts;
using System.Linq;
using TraderView.Application.Interfaces.Services;

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
        async Task<Position?> IPositionRepository.GetOpenPositionAsync(int instrumentId)
        {
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
        async Task<int> IPositionRepository.CreatePositionAsync(int instrumentId, DateTime openDate, decimal openPrice, string marketRegime)
        {
            var position = new Position
            {
                InstrumentId = instrumentId,
                OpenDate = openDate,
                Status = "Open",
                LastReportedPrice = openPrice,
                LastReportedPriceUpdated = DateTime.UtcNow,
                MarketRegime = marketRegime
            };

            await _context.Set<Position>().AddAsync(position);
            await _context.SaveChangesAsync();

            return position.Id;
        }
        async Task IPositionRepository.UpdatePositionAsync(int positionId, DateTime dateTime, decimal price, string status)
        {
            try
            {
                var position = await _context.Set<Position>().FindAsync(positionId);
                if (position != null)
                {
                    position.LastReportedPrice = price;
                    position.LastReportedPriceUpdated = dateTime;
                    position.Status = status;
                    _context.Set<Position>().Update(position);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating position with Id: {positionId}. {ex.Message}");
            }
        }
        async Task IPositionRepository.ClosePositionAsync(int positionId, DateTime closeDate)
        {
            try
            {
                var position = await _context.Set<Position>().FindAsync(positionId);
                if (position != null)
                {
                    position.CloseDate = closeDate;
                    position.Status = "Closed";
                    _context.Set<Position>().Update(position);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Closed Position (Id: {positionId}) on {closeDate:yyyy-MM-dd}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing position with Id: {positionId}. {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all positions from the database
        /// </summary>
        async Task<List<Position>> IPositionRepository.GetAllPositionsAsync()
        {
            var positions = await _context.Set<Position>()
                .AsNoTracking()
                .Include(x => x.Instrument)
                .ToListAsync();

            if (!positions.Any())
                return positions;

            // Fetch TradeExecutions for all retrieved Positions
            var positionIds = positions.Select(p => p.Id).ToList();
            var tradeExecutions = await _context.Set<TradeExecution>()
                .AsNoTracking()
                .Where(te => te.PositionId.HasValue && positionIds.Contains(te.PositionId.Value))
                .ToListAsync();

            // Group and assign TradeExecutions to their parent Position
            var executionLookup = tradeExecutions.ToLookup(te => te.PositionId ?? 0);
            foreach (var position in positions)
            {
                if (executionLookup.Contains(position.Id))
                {
                    foreach (var execution in executionLookup[position.Id])
                    {
                        position.TradeExecutions.Add(execution);
                    }
                }
            }

            return positions;
        }

        /// <summary>
        /// Gets all open positions from the database with their associated trade executions
        /// </summary>
        async Task<List<Position>> IPositionRepository.GetOpenPositionsAsync()
        {
            var positions = await _context.Set<Position>()
                .AsNoTracking()
                .Where(x => x.Status == "Open" && x.Instrument != null && x.Instrument.ContractUnitType != "CASH")
                .Include(x => x.Instrument)
                .OrderByDescending(x => x.OpenDate)
                .ToListAsync();

            if (!positions.Any())
                return positions;

            // Fetch TradeExecutions for all retrieved Positions
            var positionIds = positions.Select(p => p.Id).ToList();
            var tradeExecutions = await _context.Set<TradeExecution>()
                .AsNoTracking()
                .Where(te => te.PositionId.HasValue && positionIds.Contains(te.PositionId.Value))
                .ToListAsync();

            // Group and assign TradeExecutions to their parent Position
            var executionLookup = tradeExecutions.ToLookup(te => te.PositionId ?? 0);
            foreach (var position in positions)
            {
                if (executionLookup.Contains(position.Id))
                {
                    foreach (var execution in executionLookup[position.Id])
                    {
                        position.TradeExecutions.Add(execution);
                    }
                }
            }

            return positions;
        }        
    }
}