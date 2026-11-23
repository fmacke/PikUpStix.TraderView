using TraderView.Domain.Entities;

namespace TraderView.Application.Indicators
{
    /// <summary>
    /// Built-in technical indicator calculations without external dependencies.
    /// Supports EMA, RSI, and ATR calculations for strategy evaluation.
    /// </summary>
    public static class IndicatorCalculator
    {
        /// <summary>
        /// Calculate Exponential Moving Average
        /// </summary>
        public static List<double?> CalculateEMA(IReadOnlyList<Bar> bars, int period)
        {
            var ema = new List<double?>();
            if (bars.Count < period)
                return ema;

            double multiplier = 2.0 / (period + 1);
            double? current = null;

            for (int i = 0; i < bars.Count; i++)
            {
                if (i < period - 1)
                {
                    ema.Add(null);
                }
                else if (i == period - 1)
                {
                    // First EMA = simple average of first 'period' closes
                    current = bars.Take(period).Average(b => b.ClosePrice);
                    ema.Add(current);
                }
                else
                {
                    // EMA = (Close - Previous EMA) * Multiplier + Previous EMA
                    current = (bars[i].ClosePrice - current) * multiplier + current;
                    ema.Add(current);
                }
            }

            return ema;
        }

        /// <summary>
        /// Calculate Relative Strength Index
        /// </summary>
        public static List<double?> CalculateRSI(IReadOnlyList<Bar> bars, int period)
        {
            var rsi = new List<double?>();
            if (bars.Count < period + 1)
                return rsi;

            var gains = new List<double>();
            var losses = new List<double>();

            // Calculate gains and losses
            for (int i = 1; i < bars.Count; i++)
            {
                double change = bars[i].ClosePrice - bars[i - 1].ClosePrice;
                gains.Add(change > 0 ? change : 0);
                losses.Add(change < 0 ? Math.Abs(change) : 0);
            }

            // Fill initial NULLs
            for (int i = 0; i < period; i++)
            {
                rsi.Add(null);
            }

            double avgGain = gains.Take(period).Average();
            double avgLoss = losses.Take(period).Average();

            for (int i = period; i < bars.Count; i++)
            {
                avgGain = (avgGain * (period - 1) + gains[i - 1]) / period;
                avgLoss = (avgLoss * (period - 1) + losses[i - 1]) / period;

                double rs = avgLoss == 0 ? 100 : avgGain / avgLoss;
                double rsiValue = 100 - (100 / (1 + rs));
                rsi.Add(rsiValue);
            }

            return rsi;
        }

        /// <summary>
        /// Calculate Average True Range
        /// </summary>
        public static List<double?> CalculateATR(IReadOnlyList<Bar> bars, int period)
        {
            var atr = new List<double?>();
            if (bars.Count < period)
                return atr;

            var trueRanges = new List<double>();

            // Calculate true ranges
            for (int i = 0; i < bars.Count; i++)
            {
                double highLow = bars[i].HighPrice - bars[i].LowPrice;
                double highClose = i > 0 ? Math.Abs(bars[i].HighPrice - bars[i - 1].ClosePrice) : highLow;
                double lowClose = i > 0 ? Math.Abs(bars[i].LowPrice - bars[i - 1].ClosePrice) : highLow;

                double tr = Math.Max(highLow, Math.Max(highClose, lowClose));
                trueRanges.Add(tr);
            }

            // Fill initial NULLs
            for (int i = 0; i < period - 1; i++)
            {
                atr.Add(null);
            }

            // First ATR = simple average
            double currentATR = trueRanges.Take(period).Average();
            atr.Add(currentATR);

            // Subsequent ATRs = smoothed
            for (int i = period; i < bars.Count; i++)
            {
                currentATR = (currentATR * (period - 1) + trueRanges[i]) / period;
                atr.Add(currentATR);
            }

            return atr;
        }

        /// <summary>
        /// Calculate Simple Moving Average
        /// </summary>
        public static List<double?> CalculateSMA(IReadOnlyList<Bar> bars, int period)
        {
            var sma = new List<double?>();
            if (bars.Count < period)
                return sma;

            for (int i = 0; i < bars.Count; i++)
            {
                if (i < period - 1)
                {
                    sma.Add(null);
                }
                else
                {
                    double avg = bars.Skip(i - period + 1).Take(period).Average(b => b.ClosePrice);
                    sma.Add(avg);
                }
            }

            return sma;
        }

        /// <summary>
        /// Get the last valid (non-null) value from a series
        /// </summary>
        public static double? GetLastValue(List<double?> series)
        {
            return series.LastOrDefault(v => v.HasValue);
        }

        /// <summary>
        /// Get the value at index, or last valid if null
        /// </summary>
        public static double? GetValueOrLast(List<double?> series, int index)
        {
            if (index < 0 || index >= series.Count)
                return GetLastValue(series);

            return series[index] ?? GetLastValue(series.Take(index).ToList());
        }
    }
}
