using TraderView.Application.Indicators;
using TraderView.Domain.Backtest;
using TraderView.Domain.Entities;

namespace TraderView.Application.Strategies.Backtest
{
    /// <summary>
    /// Simple RSI Overbought/Oversold Strategy (Zero-IO example).
    /// 
    /// Rules:
    /// - Buy when RSI crosses above 30 (oversold)
    /// - Sell when RSI crosses below 70 (overbought)
    /// - Set stop-loss at entry - ATR(14) * 1.5
    /// - Set profit target at entry + ATR(14) * 2.5
    /// </summary>
    public class RSIOversoldStrategy : ITradingStrategy
    {
        private readonly int _rsiPeriod;
        private readonly int _atrPeriod;
        private readonly double _overboughtLevel;
        private readonly double _oversoldLevel;
        private readonly double _atrMultiplierStop;
        private readonly double _atrMultiplierTarget;
        private bool _inPosition;
        private PositionDirection _positionDirection;

        public string StrategyName => $"RSIOversold(RSI={_rsiPeriod}, ATR={_atrPeriod}, OB={_overboughtLevel}, OS={_oversoldLevel})";

        public RSIOversoldStrategy(
            int rsiPeriod = 14,
            int atrPeriod = 14,
            double overboughtLevel = 70,
            double oversoldLevel = 30,
            double atrMultiplierStop = 1.5,
            double atrMultiplierTarget = 2.5)
        {
            _rsiPeriod = rsiPeriod;
            _atrPeriod = atrPeriod;
            _overboughtLevel = overboughtLevel;
            _oversoldLevel = oversoldLevel;
            _atrMultiplierStop = atrMultiplierStop;
            _atrMultiplierTarget = atrMultiplierTarget;
            _inPosition = false;
            _positionDirection = PositionDirection.Long;
        }

        public List<OrderIntent> Evaluate(IReadOnlyList<Bar> bars, int currentBarIndex)
        {
            var orders = new List<OrderIntent>();

            if (bars.Count < Math.Max(_rsiPeriod, _atrPeriod) + 1)
                return orders;

            // Calculate RSI and ATR using built-in calculator
            var rsi = IndicatorCalculator.CalculateRSI(bars, _rsiPeriod);
            var atr = IndicatorCalculator.CalculateATR(bars, _atrPeriod);

            if (rsi.Count < 2 || atr.Count == 0)
                return orders;

            var currentBar = bars[^1];
            var currentPrice = currentBar.ClosePrice;
            var currentAtr = atr[^1] ?? 1.0;

            var prevRsi = rsi[^2] ?? 50;
            var currRsi = rsi[^1] ?? 50;

            bool rsiCrossingAboveOversold = prevRsi <= _oversoldLevel && currRsi > _oversoldLevel;
            bool rsiCrossingBelowOverbought = prevRsi >= _overboughtLevel && currRsi < _overboughtLevel;

            if (_inPosition)
            {
                if ((rsiCrossingBelowOverbought && _positionDirection == PositionDirection.Long) ||
                    (rsiCrossingAboveOversold && _positionDirection == PositionDirection.Short))
                {
                    _inPosition = false;
                }
            }

            if (!_inPosition && rsiCrossingAboveOversold)
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
                    PositionSize = 1.0,
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
            else if (!_inPosition && rsiCrossingBelowOverbought)
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
