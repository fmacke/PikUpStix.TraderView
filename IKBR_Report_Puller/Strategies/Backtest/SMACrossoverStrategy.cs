using TraderView.Application.Indicators;
using TraderView.Domain.Backtest;
using TraderView.Domain.Entities;

namespace TraderView.Application.Strategies.Backtest
{
    /// <summary>
    /// Simple Moving Average Crossover Strategy (Zero-IO, use case for reference).
    /// 
    /// Rules:
    /// - Buy when fast EMA crosses above slow EMA
    /// - Sell when fast EMA crosses below slow EMA
    /// - Set stop-loss at entry - ATR(14) * 2
    /// - Set profit target at entry + ATR(14) * 3
    /// </summary>
    public class SMACrossoverStrategy : ITradingStrategy
    {
        private readonly int _fastPeriod;
        private readonly int _slowPeriod;
        private readonly int _atrPeriod;
        private readonly double _atrMultiplierStop;
        private readonly double _atrMultiplierTarget;
        private bool _inPosition;
        private PositionDirection _positionDirection;

        public string StrategyName => $"SMACrossover(Fast={_fastPeriod}, Slow={_slowPeriod}, ATR={_atrPeriod})";

        public SMACrossoverStrategy(
            int fastPeriod = 12,
            int slowPeriod = 26,
            int atrPeriod = 14,
            double atrMultiplierStop = 2.0,
            double atrMultiplierTarget = 3.0)
        {
            _fastPeriod = fastPeriod;
            _slowPeriod = slowPeriod;
            _atrPeriod = atrPeriod;
            _atrMultiplierStop = atrMultiplierStop;
            _atrMultiplierTarget = atrMultiplierTarget;
            _inPosition = false;
            _positionDirection = PositionDirection.Long;
        }

        public List<OrderIntent> Evaluate(IReadOnlyList<Bar> bars, int currentBarIndex)
        {
            var orders = new List<OrderIntent>();

            // Need minimum bars for indicators
            if (bars.Count < Math.Max(_slowPeriod, _atrPeriod) + 1)
                return orders;

            // Calculate EMAs using built-in calculator
            var fastEma = IndicatorCalculator.CalculateEMA(bars, _fastPeriod);
            var slowEma = IndicatorCalculator.CalculateEMA(bars, _slowPeriod);

            if (fastEma.Count < 2 || slowEma.Count < 2)
                return orders;

            // Get ATR for stop/target sizing
            var atr = IndicatorCalculator.CalculateATR(bars, _atrPeriod);
            if (atr.Count == 0)
                return orders;

            var currentBar = bars[^1];
            var currentPrice = currentBar.ClosePrice;
            var currentAtr = atr[^1] ?? 1.0;

            var prevFastEma = fastEma[^2];
            var prevSlowEma = slowEma[^2];
            var currFastEma = fastEma[^1];
            var currSlowEma = slowEma[^1];

            if (prevFastEma == null || prevSlowEma == null || currFastEma == null || currSlowEma == null)
                return orders;

            bool fastCrossingAboveSlow = prevFastEma < prevSlowEma && currFastEma > currSlowEma;
            bool fastCrossingBelowSlow = prevFastEma > prevSlowEma && currFastEma < currSlowEma;

            // Exit signal if in position
            if (_inPosition)
            {
                if ((fastCrossingBelowSlow && _positionDirection == PositionDirection.Long) ||
                    (fastCrossingAboveSlow && _positionDirection == PositionDirection.Short))
                {
                    _inPosition = false;
                    // Return to caller; actual close happens via stop/target or liquidation
                }
            }

            // Entry signal if not in position
            if (!_inPosition && fastCrossingAboveSlow)
            {
                _inPosition = true;
                _positionDirection = PositionDirection.Long;

                double entryPrice = currentPrice;
                double stopPrice = entryPrice - (currentAtr * _atrMultiplierStop);
                double targetPrice = entryPrice + (currentAtr * _atrMultiplierTarget);

                orders.Add(new OrderIntent
                {
                    OrderType = OrderType.Entry,
                    TriggerPrice = entryPrice,
                    PositionSize = 1.0, // 1 unit; adjust per risk model
                    Direction = PositionDirection.Long,
                    PlacedAtBarIndex = currentBarIndex,
                });

                orders.Add(new OrderIntent
                {
                    OrderType = OrderType.StopLoss,
                    TriggerPrice = stopPrice,
                    PositionSize = 1.0,
                    Direction = PositionDirection.Long,
                    PlacedAtBarIndex = currentBarIndex,
                });

                orders.Add(new OrderIntent
                {
                    OrderType = OrderType.ProfitTarget,
                    TriggerPrice = targetPrice,
                    PositionSize = 1.0,
                    Direction = PositionDirection.Long,
                    PlacedAtBarIndex = currentBarIndex,
                });
            }
            else if (!_inPosition && fastCrossingBelowSlow)
            {
                _inPosition = true;
                _positionDirection = PositionDirection.Short;

                double entryPrice = currentPrice;
                double stopPrice = entryPrice + (currentAtr * _atrMultiplierStop);
                double targetPrice = entryPrice - (currentAtr * _atrMultiplierTarget);

                orders.Add(new OrderIntent
                {
                    OrderType = OrderType.Entry,
                    TriggerPrice = entryPrice,
                    PositionSize = 1.0,
                    Direction = PositionDirection.Short,
                    PlacedAtBarIndex = currentBarIndex,
                });

                orders.Add(new OrderIntent
                {
                    OrderType = OrderType.StopLoss,
                    TriggerPrice = stopPrice,
                    PositionSize = 1.0,
                    Direction = PositionDirection.Short,
                    PlacedAtBarIndex = currentBarIndex,
                });

                orders.Add(new OrderIntent
                {
                    OrderType = OrderType.ProfitTarget,
                    TriggerPrice = targetPrice,
                    PositionSize = 1.0,
                    Direction = PositionDirection.Short,
                    PlacedAtBarIndex = currentBarIndex,
                });
            }

            return orders;
        }
    }
}
