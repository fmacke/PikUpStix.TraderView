using Microsoft.EntityFrameworkCore;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Domain.Entities;
using TraderView.Infrastructure.DbContexts;
using System.Linq;

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
            return await GetOpenPositionsAsListAsync();
        }

        private async Task<List<Position>> GetOpenPositionsAsListAsync()
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

        /// <summary>
        /// Inserts or updates positions in the database
        /// </summary>
        async Task IPositionRepository.UpsertPositionsAsync(List<Position> positions)
        {
            if (positions == null || !positions.Any())
            {
                Console.WriteLine("No positions to upsert.");
                return;
            }

            int insertedCount = 0;
            int updatedCount = 0;

            foreach (var position in positions)
            {
                // Ensure instrument exists before upserting position
                if (position.Id == 0)
                {
                    await CreatePositionAsync(position.InstrumentId, position.Instrument?.DataName ?? "Unknown", position.OpenDate, position.LastReportedPrice ?? 0m);
                    insertedCount++;
                }
                else
                {
                    await UpdatePositionAsync(position.Id, DateTime.Now, position.LastReportedPrice ?? 0m, "O");
                    updatedCount++;
                }
            }
            Console.WriteLine($"Successfully processed {positions.Count} positions: {insertedCount} inserted, {updatedCount} updated.");
        }

        private async Task CreatePositionAsync(int instrumentId, string symbol, DateTime openDate, decimal openPrice)
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
        }

        private async Task UpdatePositionAsync(int positionId, DateTime dateTime, decimal price, string status)
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
    }
}