using Microsoft.EntityFrameworkCore;
using TraderView.Application.Features.Positions.Specifications;
using TraderView.Application.Features.TradeExecutions.Specifications;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Utils;
using TraderView.Domain.Entities;
using TraderView.Infrastructure.DbContexts;

namespace TraderView.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for TradeExecution-related database operations using Entity Framework Core
    /// </summary>
    public class TradeExecutionRepository : EfBaseRepository<TradeExecution>, ITradeExecutionRepository
    {
        private readonly IInstrumentService _instrumentService;
        private readonly IPositionService _positionService;
        private readonly AppDbContext _context;

        public TradeExecutionRepository(AppDbContext db, IInstrumentService instrumentService, IPositionService positionService) 
            : base(db)
        {
            _instrumentService = instrumentService;
            _positionService = positionService ?? throw new ArgumentNullException(nameof(positionService));
            _context = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <summary>
        /// Inserts or updates trade executions from a report
        /// </summary>
        async void ITradeExecutionRepository.UpsertTradeExecutions(List<TradeExecution> trades)
        {
            if (trades == null || !trades.Any())
            {
                Console.WriteLine("No trades to insert.");
                return;
            }

            foreach (var trade in trades)
            {
                string ibExecID = trade.IbExecId;
                if (string.IsNullOrEmpty(ibExecID))
                {
                    continue;
                }

                var executionExists = await TradeExistsAsync(ibExecID);

                if (!executionExists)
                {
                    try
                    {
                        trade.Position.InstrumentId = Convert.ToInt32(await _instrumentService.GetInstrumentIdByConIdAsync(trade.Conid));
                        var openPosition = await GetOpenPositionAsync(trade.Position.InstrumentId);
                        trade.PositionId = openPosition?.Id ?? await CreatePositionAsync(trade.Position.InstrumentId, trade.Symbol, trade.TradeDate, Convert.ToDecimal(trade.TradePrice), "O");
                        trade.Id = await CreateTradeExecutionAsync(trade);
                        var totalQuantity = await GetTotalQuantityForPositionAsync(trade.PositionId ?? 0);
                        if (totalQuantity == 0)
                            await ClosePositionAsync(trade.PositionId ?? 0, trade.DateTime);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error inserting trade with ibExecID {ibExecID}: {ex.Message}");
                    }
                }
                else
                {
                    var tradeExecInDb = await GetTradeExecutionByExecIDAsync(ibExecID);
                    if (tradeExecInDb != null && !tradeExecInDb.TransactionId.HasValue)
                    {
                        // Entry was made by TradeConfirmation so will be missing key details. Update the record with the new trade execution details.
                        trade.Id = tradeExecInDb.Id; // Ensure we have the correct Id for the update
                        trade.PositionId = tradeExecInDb.PositionId; // Preserve the existing PositionId
                        await UpdateTradeExecutionAsync(trade);
                    }
                }
            }
        }

        private async Task<bool> TradeExistsAsync(string ibExecID)
        {
            try
            {
                var count = await CountAsync(new TradeExecutionExistsSpecification(ibExecID));
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking existence of trade execution with ibExecID {ibExecID}: {ex.Message}");
                return false;
            }
        }

        private async Task<TradeExecution?> GetTradeExecutionByExecIDAsync(string ibExecID)
        {
            return await GetSingleAsync(new TradeExecutionByIbExecIdSpecification(ibExecID));
        }

        /// <summary>
        /// Gets all positions from the database
        /// </summary>
        List<Position> ITradeExecutionRepository.GetAllPositions()
        {
            return GetAllPositionsAsync().Result;
        }

        /// <summary>
        /// Gets all open positions from the database
        /// </summary>
        List<Position> ITradeExecutionRepository.GetOpenPositions()
        {
            return GetOpenPositionsAsync().Result;
        }

        private async Task<List<Position>> GetAllPositionsAsync()
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

        private async Task<List<Position>> GetOpenPositionsAsync()
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

        public async Task UpdateTradeExecutionAsync(TradeExecution execution)
        {
            await UpdateAsync(execution);
        }

        /// <summary>
        /// Closes a position by setting its status to 'Closed' and close date
        /// </summary>
        private async Task ClosePositionAsync(int positionId, DateTime closeDate)
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

        private async Task<Position?> GetOpenPositionAsync(int instrumentId)
        {
            try
            {
                return await _positionService.GetOpenPositionAsync(instrumentId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving open position for InstrumentId {instrumentId}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the total quantity for a position by summing all trade executions
        /// </summary>
        private async Task<decimal> GetTotalQuantityForPositionAsync(int positionId)
        {
            try
            {
                var quantity = await _context.Set<TradeExecution>()
                    .Where(te => te.PositionId == positionId)
                    .SumAsync(te => (decimal?)te.Quantity) ?? 0m;
                return quantity;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating total quantity for PositionId {positionId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets all trade executions ordered by order ID and date
        /// </summary>        
        List<TradeExecution> ITradeExecutionRepository.GetTradeExecutions()
        {
            return GetAsync(new AllTradeExecutionsSpecification()).Result.ToList();
        }

        /// <summary>
        /// Inserts or updates today's trade confirmations
        /// </summary>
        async void ITradeExecutionRepository.InsertTradeConfirmations(List<TradeConfirm> tradeConfirms)
        {
            if (tradeConfirms == null || !tradeConfirms.Any())
            {
                Console.WriteLine("No trade confirmations to process.");
                return;
            }

            foreach (var tradeConfirm in tradeConfirms)
            {
                if (string.IsNullOrEmpty(tradeConfirm.IbExecID))
                {
                    Console.WriteLine("TradeExecution confirmation missing execID. Skipping.");
                    continue;
                }

                if (!await TradeExistsAsync(tradeConfirm.IbExecID))
                {
                    var instrumentId = await _instrumentService.GetInstrumentIdByConIdAsync(tradeConfirm.Conid);

                    if (instrumentId.HasValue)
                    {
                        Position? existingPosition = null;

                        // Check for open position for the trade's symbol and instrument
                        existingPosition = await GetOpenPositionAsync(instrumentId.Value);

                        if (existingPosition != null)
                        {
                            var openDirection = existingPosition.TradeExecutions.MinBy(x => x.Id).BuySell;
                            tradeConfirm.PositionId = existingPosition.Id;
                            tradeConfirm.OpenCloseIndicator = "O";
                            if (tradeConfirm.BuySell != openDirection)
                            {
                                tradeConfirm.OpenCloseIndicator = "C";
                                if (existingPosition?.TradeExecutions.Sum(x => x.Quantity) + tradeConfirm.Quantity == 0)
                                {
                                    await ClosePositionAsync(existingPosition.Id, tradeConfirm.TradeDate);
                                }
                            }
                        }
                        else
                        {
                            tradeConfirm.OpenCloseIndicator = "O";
                            tradeConfirm.PositionId = await CreatePositionAsync(instrumentId.Value, tradeConfirm.Symbol, tradeConfirm.TradeDate, tradeConfirm.TradePrice, tradeConfirm.OpenCloseIndicator);
                        }
                        tradeConfirm.Id = await CreateTradeConfirmationAsync(tradeConfirm);
                    }
                    else
                    {
                        Console.WriteLine($"Instrument not found for symbol {tradeConfirm.Symbol} with Conid {tradeConfirm.Conid}. Skipping trade confirmation.");
                    }
                }
            }              
            Console.WriteLine("Successfully processed today's trade confirmations.");
        }
        private async Task<int> CreatePositionAsync(int instrumentId, string symbol, DateTime openDate, decimal openPrice, string openCloseIndicator)
        {
            try
            {
                var positionId = await _positionService.CreatePositionAsync(instrumentId, symbol, openDate, openPrice);
                Console.WriteLine($"Created new Position (Id: {positionId}) for symbol {symbol}, InstrumentId {instrumentId} on {openDate:yyyy-MM-dd}");
                return positionId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting position for instrumentId {instrumentId} on {openDate:yyyy-MM-dd}: {ex.Message}");
                throw;
            }
        }

        private async Task UpdatePositionAsync(int positionId, DateTime latestPriceUpdated, decimal latestPrice, string openCloseIndicator)
        {
            try
            {
                var position = await _context.Set<Position>().FindAsync(positionId);
                if (position != null)
                {
                    position.LastReportedPrice = latestPrice;
                    position.LastReportedPriceUpdated = latestPriceUpdated;
                    _context.Set<Position>().Update(position);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating position for positionId {positionId}: {ex.Message}");
                throw;
            }
        }

        private async Task<int> CreateTradeExecutionAsync(TradeExecution trade)
        {
            try
            {
                var createdTrade = await AddAsync(trade);
                Console.WriteLine($"Created new Trade Execution (Id: {createdTrade.Id}) for symbol {trade.Symbol}, on {trade.TradeDate:yyyy-MM-dd}");
                return createdTrade.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting trade execution for symbol {trade.Symbol}: {ex.Message}");
                throw;
            }
        }

        private async Task<int> CreateTradeConfirmationAsync(TradeConfirm tradeConfirm)
        {
            try
            {
                // Create a TradeExecution from the TradeConfirm
                var tradeExecution = new TradeExecution
                {
                    PositionId = tradeConfirm.PositionId,
                    Symbol = tradeConfirm.Symbol,
                    TradeDate = tradeConfirm.TradeDate,
                    DateTime = DateTime.Now,
                    Quantity = tradeConfirm.Quantity,
                    TradePrice = tradeConfirm.TradePrice,
                    BuySell = tradeConfirm.BuySell,
                    IbExecId = tradeConfirm.IbExecID,
                    Conid = tradeConfirm.Conid,
                    OpenCloseIndicator = tradeConfirm.OpenCloseIndicator
                };

                var createdTrade = await AddAsync(tradeExecution);
                Console.WriteLine($"Created new Trade Confirmation (PositionId: {tradeConfirm.PositionId}) for symbol {tradeConfirm.Symbol}, on {tradeConfirm.TradeDate:yyyy-MM-dd}");
                return createdTrade.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting trade confirmation for symbol {tradeConfirm.Symbol} with PositionId {tradeConfirm.PositionId} on {tradeConfirm.TradeDate:yyyy-MM-dd}: {ex.Message}");
                throw;
            }
        }
		/// <summary>
		/// Gets aggregated trade summary by position ID
		/// Tracks the position from opening through closing executions
		/// </summary>
		TradeSummary? ITradeExecutionRepository.GetTradeSummaryByPositionId(int positionId)
		{
			return GetTradeSummaryByPositionIdAsync(positionId).Result;
		}

		private async Task<TradeSummary?> GetTradeSummaryByPositionIdAsync(int positionId)
		{
			try
			{
				var trades = await _context.Set<TradeExecution>()
					.Where(te => te.PositionId == positionId)
					.OrderBy(te => te.TradeDate)
					.ThenBy(te => te.DateTime)
					.ToListAsync();

				if (!trades.Any())
					return null;

				var position = await _context.Set<Position>()
					.Include(p => p.Instrument)
					.FirstOrDefaultAsync(p => p.Id == positionId);

				if (position == null)
					return null;

				// Calculate running quantity and identify position closure
				decimal runningQuantity = 0;
				var closingTrade = trades.FirstOrDefault(t =>
				{
					runningQuantity += (t.Quantity ?? 0);
					return runningQuantity == 0;
				});

				var entryTrades = new List<TradeExecution>();
				var exitTrades = new List<TradeExecution>();
				runningQuantity = 0;

				foreach (var trade in trades)
				{
					runningQuantity += (trade.Quantity ?? 0);
					if ((trade.Quantity ?? 0) > 0 || ((trade.Quantity ?? 0) < 0 && runningQuantity >= 0))
					{
						entryTrades.Add(trade);
					}
					else if ((trade.Quantity ?? 0) < 0)
					{
						exitTrades.Add(trade);
					}

					if (closingTrade != null && trade.Id == closingTrade.Id)
						break;
				}

				var avgEntryPrice = entryTrades.Any() ? entryTrades.Average(t => t.TradePrice ?? 0) : 0m;
				var avgExitPrice = exitTrades.Any() ? exitTrades.Average(t => t.TradePrice ?? 0) : 0m;
				var totalQuantity = trades.Aggregate(0m, (acc, t) => acc + Math.Abs(t.Quantity ?? 0));
				var totalPnl = trades.Sum(t => t.FifoPnlRealized ?? 0);

				return new TradeSummary
				{
					InstrumentId = position.InstrumentId,
					PositionId = positionId,
					Symbol = trades.First().Symbol,
					EntryDate = trades.First().TradeDate,
					ExitDate = closingTrade?.TradeDate ?? trades.Last().TradeDate,
					EntryPrice = avgEntryPrice,
					ExitPrice = avgExitPrice,
					Quantity = totalQuantity,
					Pnl = totalPnl,
					BuySell = entryTrades.First().BuySell
				};
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error retrieving trade summary for PositionId {positionId}: {ex.Message}");
				return null;
			}
		}

		/// <summary>
		/// Gets trade executions for a specific ConId and AccountId, ordered by trade date and time
		/// </summary>
		List<(DateTime TradeDate, decimal Quantity, string OpenCloseIndicator)> ITradeExecutionRepository.GetTradeExecutionsByConIdAndAccount(long? conid, string accountId)
		{
			return GetTradeExecutionsByConIdAndAccountAsync(conid, accountId).Result;
		}

		private async Task<List<(DateTime TradeDate, decimal Quantity, string OpenCloseIndicator)>> GetTradeExecutionsByConIdAndAccountAsync(long? conid, string accountId)
		{
			try
			{
				var conidStr = conid?.ToString();
				var trades = await _context.Set<TradeExecution>()
					.Where(te => te.Conid == conidStr && te.AccountId == accountId)
					.OrderBy(te => te.TradeDate)
					.ThenBy(te => te.DateTime)
					.Select(te => new
					{
						te.TradeDate,
						te.Quantity,
						te.OpenCloseIndicator
					})
					.ToListAsync();

				return trades
					.Select(t => (t.TradeDate, t.Quantity ?? 0, t.OpenCloseIndicator ?? string.Empty))
					.ToList();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error retrieving trade executions for ConId {conid} and AccountId {accountId}: {ex.Message}");
				return new List<(DateTime TradeDate, decimal Quantity, string OpenCloseIndicator)>();
			}
		}

        /// <summary>
        /// Gets trade executions for a specific position ID
        /// </summary>
        List<TradeExecution> ITradeExecutionRepository.GetTradeExecutionsByPositionId(int positionId)
        {
            return GetAsync(new TradeExecutionByPositionIdSpecification(positionId)).Result.ToList();
        }

        /// <summary>
        /// Inserts or updates positions in the database
        /// </summary>
        void ITradeExecutionRepository.UpsertPositions(List<Position> positions)
        {
            UpsertPositionsAsync(positions).GetAwaiter().GetResult();
        }

        private async Task UpsertPositionsAsync(List<Position> positions)
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
                    await CreatePositionAsync(position.InstrumentId, position.Instrument?.DataName ?? "Unknown", position.OpenDate, position.LastReportedPrice ?? 0m, "O");
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
    }
}