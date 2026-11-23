# Backtester Quick Start Guide

## 1. Running Your First Backtest

### Via HTTP API (Recommended)

```bash
# Request
curl -X POST http://localhost:5000/api/backtests \
  -H "Content-Type: application/json" \
  -d '{
	"instrumentId": 1,
	"startDate": "2023-01-01",
	"endDate": "2024-12-31",
	"initialCapital": 10000,
	"strategyType": "SMACrossover",
	"warmupBars": 50,
	"slippagePercent": 0.01,
	"commissionPerTrade": 10.0,
	"fastPeriod": 12,
	"slowPeriod": 26,
	"atrPeriod": 14
  }'

# Response includes:
# - equity curve (daily values and drawdown)
# - all trades with entry/exit prices and P&L
# - summary stats (win rate, profit factor, total return)
```

### Programmatically (In-Process)

```csharp
// In your application code...
using TraderView.Application.Strategies.Backtest;
using TraderView.Application.UseCases.Backtest;
using TraderView.Infrastructure.DbContexts;
using TraderView.Infrastructure.Services.Backtest;

// Setup
var dbContext = new AppDbContext(options);
var barProvider = new HistoricalBarProvider(dbContext);
var executionSim = new ExecutionSimulator(slippagePercent: 0.01, commissionPerTrade: 10);
var strategy = new SMACrossoverStrategy();

// Run
var runner = new BacktestRunner(strategy, barProvider, executionSim);
var result = await runner.RunBacktestAsync(
	instrumentId: 1,
	startDate: new DateTime(2023, 1, 1),
	endDate: new DateTime(2024, 12, 31),
	initialCapital: 10000,
	warmupBars: 50);

// Inspect results
Console.WriteLine($"Trades: {result.TotalTrades}, Win Rate: {result.WinRate:P}, Return: {result.TotalReturnPercent:P}");
foreach (var trade in result.Trades)
{
	Console.WriteLine($"  {trade.EntryDate:d} → {trade.ExitDate:d}: {trade.ReturnPercent:+0.00;-0.00}%");
}
```

---

## 2. Customizing Strategies

### Add a New Strategy

```csharp
// File: IKBR_Report_Puller/Strategies/Backtest/MyStrategy.cs

using TraderView.Application.Indicators;
using TraderView.Domain.Backtest;
using TraderView.Domain.Entities;

public class MyStrategy : ITradingStrategy
{
	public string StrategyName => "MyStrategy";

	public List<OrderIntent> Evaluate(IReadOnlyList<Bar> bars, int currentBarIndex)
	{
		var orders = new List<OrderIntent>();

		// Your logic here - access bars[0..end]
		// NEVER access bars beyond the last element
		// NEVER look up external data or call DateTime.Now

		if (bars.Count < 30)
			return orders;  // Need warmup bars

		var current = bars[^1];
		var previous = bars[^2];

		// Example: Buy if close crosses above 20-day SMA
		var sma20 = IndicatorCalculator.CalculateSMA(bars, 20);
		var currentSma = sma20[^1];

		if (previous.ClosePrice <= currentSma && current.ClosePrice > currentSma)
		{
			orders.Add(new OrderIntent
			{
				OrderType = OrderType.Entry,
				TriggerPrice = current.ClosePrice,
				PositionSize = 1.0,
				Direction = PositionDirection.Long,
				PlacedAtBarIndex = currentBarIndex,
			});
		}

		return orders;
	}
}
```

### Valid Strategies

✅ **Can Do:**
- Calculate technical indicators
- Track state within `Evaluate()` call
- Access any bar from index 0 to bars.Count-1
- Return multiple orders at once

❌ **Cannot Do:**
- Call `DateTime.Now`
- Access external data sources
- Make HTTP/database requests
- Use static state between calls
- Access bars beyond the current position

---

## 3. Tweaking Execution Parameters

### Slippage
```csharp
// 0% slippage (unrealistic, academic)
new ExecutionSimulator(slippagePercent: 0.0, commissionPerTrade: 10);

// 0.1% slippage (realistic for liquid stocks)
new ExecutionSimulator(slippagePercent: 0.1, commissionPerTrade: 10);

// 0.5% slippage (tight markets, futures)
new ExecutionSimulator(slippagePercent: 0.5, commissionPerTrade: 5);
```

### Commission
```csharp
// $0 (crypto or retail zero-commission brokers)
new ExecutionSimulator(slippagePercent: 0.01, commissionPerTrade: 0);

// $5 per trade (typical discount broker)
new ExecutionSimulator(slippagePercent: 0.01, commissionPerTrade: 5);

// $25 per trade (options or futures)
new ExecutionSimulator(slippagePercent: 0.01, commissionPerTrade: 25);
```

---

## 4. Interpreting Results

### Key Metrics

| Metric | Interpretation |
|--------|-----------------|
| **Total Return %** | Overall profit/loss as % of initial capital |
| **Win Rate** | Percentage of profitable trades (target: >50%) |
| **Profit Factor** | Ratio of gross wins to gross losses (target: >1.5) |
| **Max Drawdown %** | Worst peak-to-trough decline (risk metric) |
| **Avg Win / Avg Loss** | Win/loss size ratio (target: ratio > 1.5) |
| **Bars Held** | Duration of each trade (signals overtrading if very short) |

### Red Flags

- **Win Rate 100%**: Likely overfitting or look-ahead bias
- **Max Drawdown > 30%**: Strategy may blow up in your account
- **Only 2-3 trades**: Insufficient data to assess strategy (add more instruments)
- **Return in 1 month vs. 5 years**: Survivorship bias (check extended history)

---

## 5. Performance & Optimization

### Quick Checks
```csharp
// Count trades per month
var tradesByMonth = result.Trades
	.GroupBy(t => t.EntryDate.Month)
	.Select(g => new { Month = g.Key, Count = g.Count() });

// Find your best/worst trades
var bestTrade = result.Trades.OrderByDescending(t => t.NetProfit).First();
var worstTrade = result.Trades.OrderBy(t => t.NetProfit).First();

// Profit concentration
var top10Percent = (int)Math.Ceiling(result.Trades.Count * 0.1);
var top10Trades = result.Trades
	.OrderByDescending(t => t.NetProfit)
	.Take(top10Percent)
	.Sum(t => t.NetProfit);
```

### Parameter Sensitivity
Try these nearby parameter combinations:
```csharp
// Base: EMA(12,26)
(12, 26), (10, 25), (14, 28), (12, 30), (15, 35)

// Compare results
```

---

## 6. Troubleshooting

### No trades generated
- **Cause**: Insufficient history or strategy too restrictive
- **Fix**: Check `warmupBars` is less than available bars; add debugging logs

### Unrealistic P&L
- **Cause**: Slippage/commission too low
- **Fix**: Increase `slippagePercent` and `commissionPerTrade` to realistic levels

### NuGet errors on build
- **Cause**: Old package cache
- **Fix**: `dotnet nuget locals all --clear` then rebuild

### Database connection fails
- **Cause**: AppDbContext not configured
- **Fix**: Ensure connection string in appsettings.json or user secrets

---

## 7. Integration with React Dashboard

The BacktestResult DTO is designed to render directly in your React frontend:

```javascript
// React component
const BacktestDashboard = ({ result }) => {
  return (
	<div>
	  <h1>{result.strategyMetadata.StrategyName}</h1>
	  <div class="metrics">
		<Metric label="Total Return" value={`${result.totalReturnPercent.toFixed(2)}%`} />
		<Metric label="Win Rate" value={`${(result.winRate * 100).toFixed(1)}%`} />
		<Metric label="Max Drawdown" value={`${result.maxDrawdownPercent.toFixed(2)}%`} />
	  </div>
	  <EquityCurve data={result.equityCurve} />
	  <TradeLog trades={result.trades} />
	</div>
  );
};
```

---

## 8. Next Features to Add

- [ ] Walk-forward analysis (train on 1yr, test on 1mo, rolling)
- [ ] Parameter optimization (grid search over EMA/RSI ranges)
- [ ] Monte Carlo simulation (randomize trade order)
- [ ] Sharpe ratio & Sortino ratio
- [ ] Sector/factor analysis
- [ ] Live paper trading comparison

---

**Ready to run your first backtest?** Start with the API endpoint or the example code above!
