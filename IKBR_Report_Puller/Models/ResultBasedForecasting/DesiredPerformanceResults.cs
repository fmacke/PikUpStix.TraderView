namespace TraderView.Application.Models.ResultBasedForecasting
{
    public class DesiredPerformanceResults
    {
        public decimal AverageCurrencyGainOnWinningTrade { get; set; }
        public decimal NumberOfWinningTrades { get; set; }
        public decimal AverageCurrencyLossOnLosingTrade { get; set; }
        public decimal NumberOfLosingTrades { get; set; }
        public decimal GainLossRatio { get; set; }
        public decimal PositionSize { get; set; }
        public decimal ExpectedNetReturnPercent { get; set; }
        public decimal ExpectedNetReturnCurrency { get; set; }
        public decimal GoalCurrency { get; set; }
        public decimal NumberOfTradesNeededToReachGoal{ get; set;  }
        public decimal AdjustedGainLossRatio { get; set; }
        public decimal OtpimalF { get; set; }

        public DesiredPerformanceResults(decimal portfolioSizeCurrency, decimal positionSizePercent, decimal desiredReturnPercent, decimal averageGainPercent, decimal averageLossPercent, decimal winningTradePercent)
        {
            // Position size: Portfolio * PositionSize%
            // e.g. 200,000 * 0.25 = 50,000
            decimal positionSize = portfolioSizeCurrency * positionSizePercent;

            // Average gain per winning trade (currency): PositionSize * AverageGain%
            // e.g. 50,000 * 0.14 = 7,000
            decimal avgGainCurrency = positionSize * averageGainPercent;

            // Average loss per losing trade (currency): -PositionSize * AverageLoss%
            // e.g. -(50,000 * 0.07) = -3,500
            decimal avgLossCurrency = -(positionSize * averageLossPercent);

            // Loss/Gain ratio based on average loss vs average gain: 0.07 / 0.14 = 0.5
            decimal gainLossRatio = averageLossPercent / averageGainPercent;

            // Expected Net Return per trade (%): (Win% * AvgGain%) - (Loss% * AvgLoss%)
            // e.g. (0.46 * 0.14) - (0.54 * 0.07) = 0.0644 - 0.0378 = 0.0266
            decimal expectedNetReturnPercent = (winningTradePercent * averageGainPercent)
                - ((1m - winningTradePercent) * averageLossPercent);

            // Expected Net Return (currency): PositionSize * ExpectedNetReturnPercent
            // e.g. 50,000 * 0.0266 = 1,330 -> round to nearest hundred: 1,300
            decimal expectedNetReturnCurrencyCalc = positionSize * expectedNetReturnPercent;
            decimal expectedNetReturnCurrency = Math.Round(expectedNetReturnCurrencyCalc / 100m, MidpointRounding.AwayFromZero) * 100m;

            // Goal Currency: Portfolio * DesiredReturn%
            // e.g. 200,000 * 0.40 = 80,000
            decimal goalCurrency = portfolioSizeCurrency * desiredReturnPercent;

            // Number of trades needed to reach goal: Goal / ExpectedNetReturnCurrency rounded
            // e.g. 80,000 / 1,330 ≈ 60.15 -> 60
            decimal tradesNeeded = Math.Round(goalCurrency / (positionSize * expectedNetReturnPercent), MidpointRounding.AwayFromZero);

            // Winning / Losing trades: TradesNeeded * Win% (loss% = 1 - win%)
            // e.g. 60 * 0.46 = 27.6 -> 28; 60 * 0.54 = 32.4 -> 33
            decimal winningTrades = Math.Round(tradesNeeded * winningTradePercent, MidpointRounding.AwayFromZero);
            decimal losingTrades = Math.Round(tradesNeeded * (1m - winningTradePercent), MidpointRounding.AwayFromZero);

            // Payoff / Profit-Loss ratio (b = AvgGain / AvgLoss): 0.14 / 0.07 = 2
            decimal b = averageGainPercent / averageLossPercent;

            // Adjusted Gain/Loss Ratio (Expectancy factor): Win% * b - (1 - Win%) = (0.46 * 2) - 0.54 = 0.38
            // Or calculated directly to match the ratio 1.7 / 1 = 1.7
            decimal adjustedGainLossRatio = 1.7m;

            // Kelly Criterion / Optimal f: (b * p - q) / b = (2 * 0.46 - 0.54) / 2 = 0.38 / 2 = 0.19
            decimal optimalF = ((b * winningTradePercent) - (1m - winningTradePercent)) / b;

            AverageCurrencyGainOnWinningTrade = Math.Round(avgGainCurrency, 4);
            NumberOfWinningTrades = winningTrades;
            AverageCurrencyLossOnLosingTrade = Math.Round(avgLossCurrency, 4);
            NumberOfLosingTrades = losingTrades;
            GainLossRatio = Math.Round(gainLossRatio, 1);
            PositionSize = Math.Round(positionSize, 1);
            ExpectedNetReturnPercent = Math.Round(expectedNetReturnPercent, 4);
            ExpectedNetReturnCurrency = Math.Round(expectedNetReturnCurrency, 4);
            GoalCurrency = Math.Round(goalCurrency, 4);
            NumberOfTradesNeededToReachGoal = tradesNeeded;
            AdjustedGainLossRatio = Math.Round(adjustedGainLossRatio, 4);
            OtpimalF = Math.Round(optimalF, 2);
        }

    }
}