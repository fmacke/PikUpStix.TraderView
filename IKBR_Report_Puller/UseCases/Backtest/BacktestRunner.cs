using TraderView.Domain.Backtest;
using TraderView.Domain.Entities;

namespace TraderView.Application.UseCases.Backtest
{
    /// <summary>
    /// Main backtesting orchestrator.
    /// Implements the three-step bar-by-bar loop:
    ///   Step A: Fill pending orders (check stops, targets, entries)
    ///   Step B: Evaluate strategy on current bars, generate new orders
    ///   Step C: Mark-to-market and record equity
    /// </summary>
    public class BacktestRunner
    {
        private readonly ITradingStrategy _strategy;
        private readonly IHistoricalBarProvider _barProvider;
        private readonly ExecutionSimulator _executionSimulator;
        private readonly TradeBuffer _tradeBuffer;

        public BacktestRunner(
            ITradingStrategy strategy,
            IHistoricalBarProvider barProvider,
            ExecutionSimulator executionSimulator)
        {
            _strategy = strategy;
            _barProvider = barProvider;
            _executionSimulator = executionSimulator;
            _tradeBuffer = new TradeBuffer();
        }

        /// <summary>
        /// Run a complete backtest on the given instrument and date range.
        /// </summary>
        public async Task<BacktestResult> RunBacktestAsync(
            int instrumentId,
            DateTime startDate,
            DateTime endDate,
            double initialCapital = 10000.0,
            int warmupBars = 50)
        {
            // Fetch historical bars
            var allBars = await _barProvider.GetBarsAsync(instrumentId, startDate, endDate);

            if (allBars.Count < warmupBars + 1)
            {
                throw new InvalidOperationException(
                    $"Insufficient bars for backtest. Need {warmupBars + 1}, got {allBars.Count}");
            }

            var result = new BacktestResult
            {
                InstrumentName = allBars.First().InstrumentId.ToString(), // TODO: resolve instrument name
                InitialCapital = initialCapital,
                BacktestStartDate = allBars[warmupBars].Date,
                BacktestEndDate = allBars[^1].Date,
                StrategyMetadata = new Dictionary<string, string>
                {
                    { "StrategyName", _strategy.StrategyName },
                    { "WarmupBars", warmupBars.ToString() },
                }
            };

            double cash = initialCapital;
            double peakEquity = initialCapital;
            var dailyEquities = new Dictionary<DateTime, double>();

            // Main backtest loop: process each bar from warmupBars onward
            for (int i = warmupBars; i < allBars.Count; i++)
            {
                var currentBar = allBars[i];
                var previousBar = i > 0 ? allBars[i - 1] : null;

                // Step A: Fill pending orders from previous bar
                var fills = _tradeBuffer.ProcessBar(
                    barOpen: currentBar.OpenPrice,
                    barHigh: currentBar.HighPrice,
                    barLow: currentBar.LowPrice,
                    barClose: currentBar.ClosePrice,
                    barIndex: i,
                    generateFillPrice: (open, trigger) =>
                    {
                        // Simulate realistic fill: at open plus slippage
                        return _executionSimulator.GetFillPrice(open, trigger, currentBar.HighPrice, currentBar.LowPrice);
                    });

                // Record newly closed trades
                foreach (var (closedTrade, _) in fills)
                {
                    if (closedTrade != null)
                    {
                        closedTrade.Commission = _executionSimulator.GetCommission(closedTrade.Quantity, closedTrade.EntryPrice);
                        closedTrade.NetProfit = closedTrade.GrossProfit - closedTrade.Commission;
                        closedTrade.ReturnPercent = (closedTrade.NetProfit / (closedTrade.EntryPrice * closedTrade.Quantity)) * 100;

                        // Update cash
                        cash -= closedTrade.Commission;
                        if (closedTrade.Direction == PositionDirection.Long)
                        {
                            cash += closedTrade.GrossProfit;
                        }
                        else
                        {
                            cash += closedTrade.GrossProfit;
                        }
                    }
                }

                // Step B: Evaluate strategy on bars up to and including current bar
                var lookbackBars = allBars.GetRange(Math.Max(0, i - 100), i - Math.Max(0, i - 100) + 1);
                var orders = _strategy.Evaluate(lookbackBars, i);
                _tradeBuffer.PlaceOrders(orders, i);

                // Step C: Mark-to-market and record equity curve
                double openPositionsValue = _tradeBuffer.GetPortfolioEquity(currentBar.ClosePrice);
                double portfolioEquity = cash + openPositionsValue;

                if (!dailyEquities.ContainsKey(currentBar.Date.Date))
                {
                    dailyEquities[currentBar.Date.Date] = portfolioEquity;
                }

                // Track peak for drawdown calculation
                if (portfolioEquity > peakEquity)
                {
                    peakEquity = portfolioEquity;
                }

                double drawdown = peakEquity - portfolioEquity;
                double drawdownPercent = (drawdown / peakEquity) * 100;

                result.EquityCurve.Add(new EquityPoint
                {
                    Date = currentBar.Date,
                    Equity = portfolioEquity,
                    DrawdownPercent = drawdownPercent,
                });
            }

            // Liquidate all remaining positions at final close
            var lastBar = allBars[^1];
            _tradeBuffer.LiquidateAll(lastBar.ClosePrice, allBars.Count - 1);

            // Finalize results
            result.Trades.AddRange(_tradeBuffer.ClosedTrades);
            result.FinalCapital = cash;
            result.TotalReturn = result.FinalCapital - result.InitialCapital;
            result.TotalReturnPercent = (result.TotalReturn / result.InitialCapital) * 100;

            // Calculate statistics
            CalculateTradeStatistics(result);

            return result;
        }

        /// <summary>
        /// Calculate P&L statistics from closed trades.
        /// </summary>
        private void CalculateTradeStatistics(BacktestResult result)
        {
            result.TotalTrades = result.Trades.Count;
            result.WinningTrades = result.Trades.Count(t => t.NetProfit > 0);
            result.LosingTrades = result.Trades.Count(t => t.NetProfit < 0);
            result.WinRate = result.TotalTrades > 0 ? (double)result.WinningTrades / result.TotalTrades : 0;

            var winningTrades = result.Trades.Where(t => t.NetProfit > 0).ToList();
            var losingTrades = result.Trades.Where(t => t.NetProfit < 0).ToList();

            result.AvgWin = winningTrades.Any() ? winningTrades.Average(t => t.NetProfit) : 0;
            result.AvgLoss = losingTrades.Any() ? losingTrades.Average(t => t.NetProfit) : 0;

            double grossWins = winningTrades.Sum(t => t.NetProfit);
            double grossLosses = losingTrades.Sum(t => Math.Abs(t.NetProfit));
            result.ProfitFactor = grossLosses > 0 ? grossWins / grossLosses : 0;

            result.TotalCommission = result.Trades.Sum(t => t.Commission);

            // Max Drawdown
            if (result.EquityCurve.Count > 0)
            {
                result.MaxDrawdown = result.EquityCurve.Max(e => e.Equity) - result.EquityCurve.Min(e => e.Equity);
                double maxEquity = result.EquityCurve.Max(e => e.Equity);
                result.MaxDrawdownPercent = maxEquity > 0
                    ? (result.MaxDrawdown / maxEquity) * 100
                    : 0;
            }
        }
    }

    /// <summary>
    /// Interface for fetching historical bars.
    /// Abstraction to allow swapping data sources (DB, CSV, API).
    /// </summary>
    public interface IHistoricalBarProvider
    {
        Task<List<Bar>> GetBarsAsync(int instrumentId, DateTime startDate, DateTime endDate);
    }

    /// <summary>
    /// Simulates realistic execution (slippage + commission).
    /// </summary>
    public class ExecutionSimulator
    {
        private readonly double _slippagePercent;
        private readonly double _commissionPerTrade;

        public ExecutionSimulator(double slippagePercent = 0.01, double commissionPerTrade = 10.0)
        {
            _slippagePercent = slippagePercent;
            _commissionPerTrade = commissionPerTrade;
        }

        /// <summary>
        /// Calculate fill price with slippage applied.
        /// </summary>
        public double GetFillPrice(double barOpen, double triggerPrice, double barHigh, double barLow)
        {
            // Simple model: fill at next bar open +/- slippage
            double basePrice = barOpen;
            double slippage = basePrice * (_slippagePercent / 100.0);

            // If trigger is above open, assume we're buying (slippage up), otherwise selling (slippage down)
            if (triggerPrice > basePrice)
                return basePrice + slippage;
            else
                return basePrice - slippage;
        }

        /// <summary>
        /// Calculate commission based on trade size.
        /// </summary>
        public double GetCommission(double quantity, double price)
        {
            return _commissionPerTrade;
        }
    }
}
