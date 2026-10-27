using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Models;
using TraderView.Application.Utils;
using TraderView.Domain.Entities;
using System.Diagnostics;

namespace TraderView.Application.Services
{
    /// <summary>
    /// Service responsible for trading related reports. This implementation follows the application's
    /// repository/specification pattern by depending on repositories (no direct DbContext usage).
    /// It currently implements market regime detection used elsewhere in the application.
    /// </summary>
    public class TradingReportService : ITradingReportService
    {
        private readonly IHistoricalDataService _historicalDataService;
        private readonly IPositionService _positionService;
        private readonly ITradeExecutionService _tradeExecutionService;
        private readonly ILogger<TradingReportService> _logger;
        

        public TradingReportService(IHistoricalDataService historicalDataService, IPositionService positionService, ITradeExecutionService tradeExecutionService, ILogger<TradingReportService> logger)
        {
            _historicalDataService = historicalDataService ?? throw new ArgumentNullException(nameof(historicalDataService));
            _positionService = positionService ?? throw new ArgumentNullException(nameof(positionService));
            _tradeExecutionService = tradeExecutionService ?? throw new ArgumentNullException(nameof(tradeExecutionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        

        /// <summary>
        /// Calculate position outcomes (MFE / MAE) for all positions.
        /// This aggregates position/trade data and pulls historical bars to compute excursions.
        /// </summary>
        public async Task<List<PositionOutcome>> CalculatePositionOutcomesAsync(CancellationToken cancellationToken = default)
        {
            var results = new List<PositionOutcome>();

            var sw = Stopwatch.StartNew();
            _logger.LogInformation("Starting CalculatePositionOutcomesAsync");

            var positions = await _positionService.GetAllPositionsAsync(true);
            if (positions == null || !positions.Any())
            {
                _logger.LogInformation("No positions found");
                return results;
            }

            _logger.LogInformation("Found {Count} positions, beginning processing", positions.Count);

            int idx = 0;
            foreach (var pos in positions)
            {
                idx++;
                try
                {
                    if (idx % 25 == 0)
                    {
                        _logger.LogInformation("Processing position {Index}/{Total} (Id={PositionId})", idx, positions.Count, pos.Id);
                    }

                    var outcome = await ((ITradingReportService)this).GetPositionReport(pos);
                    if (outcome == null)
                    {
                        continue;
                    }

                    results.Add(outcome);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing position {PositionId}", pos?.Id);
                }
            }

            sw.Stop();
            _logger.LogInformation("CalculatePositionOutcomesAsync completed in {Elapsed}ms, processed {Count} positions", sw.ElapsedMilliseconds, results.Count);

            return results;
        }

        public async Task<List<OpenPositionReportData>> PrepareOpenPositionReportData(IReadOnlyList<Position> openPositions)
        {
            var reportDataList = new List<OpenPositionReportData>();

            foreach (var position in openPositions)
            {
                try
                {
                    string accountId = position.TradeExecutions.First().AccountId;
                    string symbol = position.TradeExecutions.First().Symbol;
                    string currency = position.TradeExecutions.First().Currency;
                    long? conid = TypeConverters.ConvertToLong(position.Instrument.ConId);
                    decimal? currentPositionQuantity = position.TradeExecutions.Sum(x => x.Quantity);
                    decimal? costBasisPrice = CalculateAverageCost(position.TradeExecutions);
                    decimal? positionValue = position.LastReportedPrice * currentPositionQuantity;
                    decimal? unrealizedPnL = (position.LastReportedPrice - costBasisPrice) * currentPositionQuantity;
                    DateTime? dateOpened = position.OpenDate;
                    int? daysOpened = dateOpened.HasValue
                        ? (int)(DateTime.Today - dateOpened.Value.Date).TotalDays
                        : null;

                    decimal? averagePrice = currentPositionQuantity != 0
                        ? positionValue / currentPositionQuantity
                        : 0;

                    decimal? percentChange = costBasisPrice != 0
                        ? (averagePrice - costBasisPrice) / costBasisPrice
                        : 0;

                    decimal? currentMargin = positionValue - (currentPositionQuantity * costBasisPrice);

                    reportDataList.Add(new OpenPositionReportData
                    {
                        PositionId = position.Id,
                        AccountId = accountId,
                        Symbol = symbol,
                        DateOpened = dateOpened,
                        DaysOpened = daysOpened,
                        Quantity = currentPositionQuantity,
                        CostPrice = costBasisPrice,
                        AveragePrice = averagePrice,
                        Value = positionValue,
                        UnrealizedPnL = unrealizedPnL,
                        PercentChange = percentChange,
                        CurrentMargin = currentMargin,
                        Currency = currency
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in PrepareOpenPositionReportData processing position {PositionId}", position?.Id);
                }
            }

            return reportDataList;
        }

        private static decimal? CalculateAverageCost(ICollection<TradeExecution> tradeExecutions)
        {
            if (tradeExecutions == null || !tradeExecutions.Any())
            {
                return 0;
            }

            decimal? totalCost = 0;
            decimal? totalQuantity = tradeExecutions.Where(x => x.OpenCloseIndicator.Contains("O")).Sum(x => x.Quantity);
            foreach (var tradeExecution in tradeExecutions.Where(x => x.OpenCloseIndicator.Contains("O")))
            {
                if (tradeExecution.TradePrice == null || tradeExecution.Quantity == null)
                {
                    totalCost += 0;
                    continue;
                }

                totalCost += tradeExecution.TradePrice * tradeExecution.Quantity;
            }

            return totalQuantity != 0 ? totalCost / totalQuantity : 0;
        }

        async Task<PositionOutcome?> ITradingReportService.GetPositionReport(Position pos)
        {
            // Get aggregated trade summary for the position (entry/exit dates/prices)
            var summary = await _tradeExecutionService.GetTradeSummaryByPositionIdAsync(pos.Id);
            if (summary == null)
                return null;

            var entryDate = summary.EntryDate;
            var exitDate = summary.ExitDate == default ? DateTime.UtcNow : summary.ExitDate;

            // Pull historical bars for the instrument between entry and exit (inclusive)
            var bars = await _historicalDataService.GetCandlesticksAsync(summary.InstrumentId, entryDate.AddDays(-1), exitDate.AddDays(1));

            decimal mfe = 0m;
            decimal mae = 0m;

            if (bars != null && bars.Any())
            {
                var maxHigh = bars.Max(b => (decimal)b.HighPrice);
                var minLow = bars.Min(b => (decimal)b.LowPrice);

                // Normalize buy/sell direction - assume Buy (long) when BuySell starts with 'B'
                var isBuy = !string.IsNullOrWhiteSpace(summary.BuySell) && summary.BuySell.StartsWith("B", StringComparison.OrdinalIgnoreCase);

                if (isBuy)
                {
                    mfe = Math.Max(0, maxHigh - summary.EntryPrice);
                    mae = Math.Max(0, summary.EntryPrice - minLow);
                }
                else
                {
                    // Short: favourable when price falls
                    mfe = Math.Max(0, summary.EntryPrice - minLow);
                    mae = Math.Max(0, maxHigh - summary.EntryPrice);
                }
            }

            var outcome = new PositionOutcome
            {
                Id = pos.Id,
                OpenDate = pos.OpenDate,
                CloseDate = pos.CloseDate,
                Status = pos.Status,
                InstrumentId = pos.InstrumentId,
                LastReportedPrice = pos.LastReportedPrice,
                LastReportedPriceUpdated = pos.LastReportedPriceUpdated,
                MarketRegime = pos.MarketRegime,
                MaximumFavourableExcursion = mfe,
                MaximumAdverseExcursion = mae,
                // prefer symbol from summary if available, otherwise fall back to related instrument name
                SymbolName = !string.IsNullOrWhiteSpace(summary.Symbol) ? summary.Symbol : pos.Instrument?.InstrumentName,
                // Only set final margin for closed positions
                Margin = pos.CloseDate != null ? summary.Pnl : (decimal?)null
            };
            return null;
        }
    }
}
