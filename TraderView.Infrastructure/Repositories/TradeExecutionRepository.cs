using Microsoft.EntityFrameworkCore;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Specifications.TradeExecutions;
using TraderView.Domain.Entities;
using TraderView.Infrastructure.DbContexts;

namespace TraderView.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for TradeExecution-related database operations using Entity Framework Core
    /// </summary>
    public class TradeExecutionRepository : EfBaseRepository<TradeExecution>, ITradeExecutionRepository
    {
        private readonly AppDbContext _context;

        public TradeExecutionRepository(AppDbContext db) 
            : base(db)
        {
            _context = db ?? throw new ArgumentNullException(nameof(db));
        }

        async Task<bool> ITradeExecutionRepository.TradeExecutionExistsAsync(string ibExecID)
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

        async Task<TradeExecution?> ITradeExecutionRepository.GetTradeExecutionByExecIDAsync(string ibExecID)
        {
            return await GetSingleAsync(new TradeExecutionByIbExecIdSpecification(ibExecID));
        }

        /// <summary>
        /// Gets the total quantity for a position by summing all trade executions
        /// </summary>
        async Task<decimal> ITradeExecutionRepository.GetTotalQuantityForPositionAsync(int positionId)
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
        async Task<List<TradeExecution>> ITradeExecutionRepository.GetTradeExecutionsAsync()
        {
            return (await GetAsync(new AllTradeExecutionsSpecification())).ToList();
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
        async Task<int> ITradeExecutionRepository.CreateTradeConfirmationAsync(TradeConfirm tradeConfirm)
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
		async Task<TradeSummary?> ITradeExecutionRepository.GetTradeSummaryByPositionIdAsync(int positionId)
		{
			return await GetTradeSummaryByPositionIdAsync(positionId);
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
		async Task<List<(DateTime TradeDate, decimal Quantity, string OpenCloseIndicator)>> ITradeExecutionRepository.GetTradeExecutionsByConIdAndAccountAsync(long? conid, string accountId)
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
        async Task<List<TradeExecution>> ITradeExecutionRepository.GetTradeExecutionsByPositionIdAsync(int positionId)
        {
            return (await GetAsync(new TradeExecutionByPositionIdSpecification(positionId))).ToList();
        }
    }
}