# PikUpStix.TraderView Backtester - Implementation Summary

## Overview
A complete bar-by-bar backtesting framework has been successfully implemented across your .NET solution. The backtester follows a clean layered architecture with zero external technical indicator dependencies, using built-in calculation logic instead.

## Architecture

### 1. Domain Layer (TraderView.Domain/Backtest)
Core domain models defining the backtesting contracts:

- **Signal.cs** - Enum for trading signals (BuySignal, SellSignal, BuyCoverSignal, NoSignal)
- **OrderIntent.cs** - Pending order model with order type, trigger price, position size, and direction
- **BacktestTrade.cs** - Closed trade record including entry/exit prices, P&L, and duration
- **BacktestResult.cs** - Complete backtest summary with equity curve and trade statistics
- **ITradingStrategy.cs** - Core interface for pure strategies (zero-IO, no side effects, look-ahead safe)

### 2. Application Layer (IKBR_Report_Puller)

#### Domain Models
- **ITradingStrategy.cs** - Interface in Domain but instantiated in Application

#### Indicator Calculator
- **IndicatorCalculator.cs** - Built-in calculations for:
  - EMA (Exponential Moving Average)
  - RSI (Relative Strength Index)
  - ATR (Average True Range)
  - SMA (Simple Moving Average)
  - No external dependencies required

#### Reference Strategies
Two example strategies demonstrating the ITradingStrategy contract:

- **SMACrossoverStrategy.cs**
  - Buy: fast EMA crosses above slow EMA
  - Sell: fast EMA crosses below slow EMA
  - Stop-loss: entry - ATR(14) × 2
  - Profit target: entry + ATR(14) × 3
  - Parameters: FastPeriod=12, SlowPeriod=26, AtrPeriod=14

- **RSIOversoldStrategy.cs**
  - Buy: RSI crosses above 30 (oversold)
  - Sell: RSI crosses below 70 (overbought)
  - Stop-loss: entry - ATR(14) × 1.5
  - Profit target: entry + ATR(14) × 2.5
  - Parameters: RsiPeriod=14, AtrPeriod=14

#### Orchestration (UseCases/Backtest)

- **TradeBuffer.cs**
  - Manages pending orders (entry, stop-loss, profit-target)
  - Tracks active positions
  - Processes bar High/Low for order fills
  - Implements position lifecycle (open → close with P&L calculation)
  - Built-in support for stop-loss and profit-target checking

- **BacktestRunner.cs**
  - Main orchestrator implementing the three-step bar-by-bar loop:
	1. **Step A - Fill Orders**: Check stops, targets, and new entries against bar High/Low
	2. **Step B - Evaluate Strategy**: Call strategy.Evaluate() with pruned lookback bars
	3. **Step C - Mark-to-Market**: Record daily equity and calculate drawdown
  - Includes ExecutionSimulator for realistic fills (slippage + commission)
  - Computes final trade statistics (win rate, profit factor, max drawdown)

### 3. Infrastructure Layer (TraderView.Infrastructure)

- **HistoricalBarProvider.cs**
  - Queries historical bars from AppDbContext
  - Converts HistoricalDatum entities to Bar domain model
  - Built-in caching to avoid repeated DB queries during multiple backtests
  - Implements IHistoricalBarProvider interface

### 4. API Layer (traderview.Server)

- **BacktestController.cs**
  - POST `/api/backtests` endpoint
  - Accepts BacktestRequest with:
	- Instrument ID, date range, initial capital
	- Strategy type (SMACrossover, RSIOversold, or custom)
	- Execution parameters (slippage %, commission, warmup bars)
	- Strategy-specific parameters (EMA periods, RSI levels, etc.)
  - Returns BacktestResult DTO with:
	- Trade log (entry/exit prices, P&L)
	- Equity curve (daily equity + drawdown %)
	- Summary statistics (win rate, profit factor, total return)

## Key Features

### ✅ Zero I/O Strategy Contracts
Strategies receive only historical bars and generate orders. No database calls, API requests, or DateTime.Now lookups. Same strategy code runs in backtest and live trading.

### ✅ Look-Ahead Safe
BacktestRunner passes only bars[0..i] to strategies, preventing accidental use of future data.

### ✅ Realistic Fill Simulation
- Market orders fill at next bar's open with configurable slippage
- Stop-losses and profit-targets checked against bar's High/Low
- Conservative fill pricing (worse price favoring the market)
- Per-trade commission support

### ✅ Comprehensive P&L Tracking
- Gross profit, commission, net profit
- Return percentage and basis points
- Maximum Favorable/Adverse Excursion
- Daily equity curve with drawdown calculation
- Win rate, profit factor, average win/loss

### ✅ No External Dependencies
Built-in technical indicator calculations (EMA, RSI, ATR, SMA) mean:
- No NuGet package resolution issues
- Lightweight and fast
- Transparent algorithm implementation
- Easy to understand and customize

## Usage Example

```csharp
// 1. Create strategy instance
var strategy = new SMACrossoverStrategy(
	fastPeriod: 12,
	slowPeriod: 26,
	atrPeriod: 14);

// 2. Create data provider
var barProvider = new HistoricalBarProvider(dbContext);

// 3. Create execution simulator
var simulator = new ExecutionSimulator(
	slippagePercent: 0.01,  // 0.01% slippage
	commissionPerTrade: 10); // $10 per trade

// 4. Run backtest
var runner = new BacktestRunner(strategy, barProvider, simulator);
var result = await runner.RunBacktestAsync(
	instrumentId: 1,
	startDate: new DateTime(2023, 1, 1),
	endDate: new DateTime(2024, 12, 31),
	initialCapital: 10000,
	warmupBars: 50);

// 5. Access results
Console.WriteLine($"Total Trades: {result.TotalTrades}");
Console.WriteLine($"Win Rate: {result.WinRate:P}");
Console.WriteLine($"Total Return: {result.TotalReturnPercent:P}");
Console.WriteLine($"Max Drawdown: {result.MaxDrawdownPercent:P}");
```

## API Endpoint

```http
POST /api/backtests
Content-Type: application/json

{
  "instrumentId": 1,
  "startDate": "2023-01-01T00:00:00",
  "endDate": "2024-12-31T00:00:00",
  "initialCapital": 10000,
  "strategyType": "SMACrossover",
  "warmupBars": 50,
  "slippagePercent": 0.01,
  "commissionPerTrade": 10.0,
  "fastPeriod": 12,
  "slowPeriod": 26,
  "atrPeriod": 14
}

Response:
{
  "instrumentName": "1",
  "initialCapital": 10000,
  "finalCapital": 12500,
  "totalReturn": 2500,
  "totalReturnPercent": 25,
  "maxDrawdown": 1200,
  "maxDrawdownPercent": 8.5,
  "totalTrades": 24,
  "winningTrades": 15,
  "losingTrades": 9,
  "winRate": 0.625,
  "avgWin": 450,
  "avgLoss": -200,
  "profitFactor": 2.25,
  "totalCommission": 240,
  "backtestStartDate": "2023-02-19T00:00:00",
  "backtestEndDate": "2024-12-31T00:00:00",
  "equityCurve": [
	{ "date": "2023-02-19T00:00:00", "equity": 10000, "drawdownPercent": 0 },
	{ "date": "2023-02-20T00:00:00", "equity": 10250, "drawdownPercent": 0 },
	...
  ],
  "trades": [
	{
	  "entryBarIndex": 50,
	  "entryDate": "2023-02-19T00:00:00",
	  "entryPrice": 150.5,
	  "direction": 1,
	  "quantity": 1.0,
	  "exitBarIndex": 65,
	  "exitDate": "2023-03-05T00:00:00",
	  "exitPrice": 155.2,
	  "exitType": 0,
	  "grossProfit": 4.7,
	  "commission": 10,
	  "netProfit": -5.3,
	  "returnPercent": -3.52,
	  "maxFavorableExcursion": 8.5,
	  "maxAdverseExcursion": -2.1,
	  "barsHeld": 15
	},
	...
  ],
  "strategyMetadata": {
	"StrategyName": "SMACrossover(Fast=12, Slow=26, ATR=14)",
	"WarmupBars": "50"
  }
}
```

## Creating Custom Strategies

Implement `ITradingStrategy`:

```csharp
public class MyCustomStrategy : ITradingStrategy
{
	public string StrategyName => "MyCustomStrategy";

	public List<OrderIntent> Evaluate(IReadOnlyList<Bar> bars, int currentBarIndex)
	{
		var orders = new List<OrderIntent>();

		if (bars.Count < 50)
			return orders;  // Need minimum warmup

		// Access historical data
		var ema = IndicatorCalculator.CalculateEMA(bars, 20);
		var rsi = IndicatorCalculator.CalculateRSI(bars, 14);
		var atr = IndicatorCalculator.CalculateATR(bars, 14);

		var current = bars[^1];
		var currentEma = ema[^1];
		var currentRsi = rsi[^1];

		// Generate orders based on analysis
		if (current.ClosePrice > currentEma && currentRsi < 30)
		{
			double entryPrice = current.ClosePrice;
			double currentAtr = atr[^1] ?? 1.0;

			orders.Add(new OrderIntent
			{
				OrderType = OrderType.Entry,
				TriggerPrice = entryPrice,
				PositionSize = 1.0,
				Direction = PositionDirection.Long,
				PlacedAtBarIndex = currentBarIndex,
			});

			// Add stop and target orders
			orders.Add(new OrderIntent
			{
				OrderType = OrderType.StopLoss,
				TriggerPrice = entryPrice - (currentAtr * 2),
				PositionSize = 1.0,
				Direction = PositionDirection.Long,
				PlacedAtBarIndex = currentBarIndex,
			});

			orders.Add(new OrderIntent
			{
				OrderType = OrderType.ProfitTarget,
				TriggerPrice = entryPrice + (currentAtr * 3),
				PositionSize = 1.0,
				Direction = PositionDirection.Long,
				PlacedAtBarIndex = currentBarIndex,
			});
		}

		return orders;
	}
}

// Use in BacktestController:
// Instantiate and pass to BacktestRunner
var strategy = new MyCustomStrategy();
var runner = new BacktestRunner(strategy, barProvider, simulator);
var result = await runner.RunBacktestAsync(...);
```

## Files Created

```
TraderView.Domain/
├── Backtest/
│   ├── Signal.cs
│   ├── OrderIntent.cs
│   ├── BacktestTrade.cs
│   ├── BacktestResult.cs
│   └── ITradingStrategy.cs

IKBR_Report_Puller/
├── Indicators/
│   └── IndicatorCalculator.cs
├── Strategies/Backtest/
│   ├── SMACrossoverStrategy.cs
│   └── RSIOversoldStrategy.cs
└── UseCases/Backtest/
	├── TradeBuffer.cs
	└── BacktestRunner.cs

TraderView.Infrastructure/
└── Services/Backtest/
	└── HistoricalBarProvider.cs

traderview/traderview.Server/
└── Controllers/
	└── BacktestController.cs
```

## Build Status
✅ **All projects compile successfully** (0 errors, 33 warnings from frontend dependencies)

## Next Steps

1. **Add unit tests** - Create test strategies and mock data providers
2. **Enhance indicator library** - Add Bollinger Bands, MACD, Stochastic
3. **React dashboard integration** - Render equity curve and trade log
4. **Parameter optimization** - Grid search over strategy parameters
5. **Walk-forward analysis** - Split data into train/test windows
6. **Performance profiling** - Optimize for 1000+ bar backtests

## Notes

- All strategies are stateless within each evaluation call - no side effects or mutable state retained between executions
- The same strategy class works unchanged in live trading by simply passing recent bars instead of historical
- Backtests can handle any bar frequency (minute, hourly, daily, weekly) depending on your data
- Commission model is simplified (flat fee per trade); easily customizable for percentage-based or tiered models
