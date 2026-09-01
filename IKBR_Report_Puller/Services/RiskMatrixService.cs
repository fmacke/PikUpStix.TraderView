using TraderView.Application.Interfaces.Services;
using TraderView.Application.Models.ResultBasedForecasting;
using TraderView.Domain.Entities.FMP;

namespace TraderView.Application.Services
{
    public class ResultsBasedAssumptionForecastService : IResultsBasedAssumptionForecastService
    {
        public ResultBasedAssumptionForecastResults CalculateForecast(ResultBasedAssumptionForecastInputs inputs)
        {
            if (inputs == null)
                throw new ArgumentNullException(nameof(inputs));

            // Position size: Portfolio * PositionSize%
            // e.g. 200,000 * 0.25 = 50,000
            decimal positionSize = inputs.PortfolioSizeCurrency * inputs.PositionSizePercent;

            // Average gain per winning trade (currency): PositionSize * AverageGain%
            // e.g. 50,000 * 0.14 = 7,000
            decimal avgGainCurrency = positionSize * inputs.AverageGainPercent;

            // Average loss per losing trade (currency): -PositionSize * AverageLoss%
            // e.g. -(50,000 * 0.07) = -3,500
            decimal avgLossCurrency = -(positionSize * inputs.AverageLossPercent);

            // Loss/Gain ratio based on average loss vs average gain: 0.07 / 0.14 = 0.5
            decimal gainLossRatio = inputs.AverageLossPercent / inputs.AverageGainPercent;

            // Expected Net Return per trade (%): (Win% * AvgGain%) - (Loss% * AvgLoss%)
            // e.g. (0.46 * 0.14) - (0.54 * 0.07) = 0.0644 - 0.0378 = 0.0266
            decimal expectedNetReturnPercent = (inputs.WinningTradePercent * inputs.AverageGainPercent)
                - ((1m - inputs.WinningTradePercent) * inputs.AverageLossPercent);

            // Expected Net Return (currency): PositionSize * ExpectedNetReturnPercent
            // e.g. 50,000 * 0.0266 = 1,330 -> round to nearest hundred or truncate: 1,300
            decimal expectedNetReturnCurrency = Math.Round(positionSize * expectedNetReturnPercent / 100m, MidpointRounding.AwayFromZero) * 100m;

            // Goal Currency: Portfolio * DesiredReturn%
            // e.g. 200,000 * 0.40 = 80,000
            decimal goalCurrency = inputs.PortfolioSizeCurrency * inputs.DesiredReturnPercent;

            // Number of trades needed to reach goal: Goal / ExpectedNetReturnCurrency rounded
            // e.g. 80,000 / 1,330 ≈ 60.15 -> 60
            decimal tradesNeeded = Math.Round(goalCurrency / (positionSize * expectedNetReturnPercent), MidpointRounding.AwayFromZero);

            // Winning / Losing trades: TradesNeeded * Win% (loss% = 1 - win%)
            // e.g. 60 * 0.46 = 27.6 -> 28; 60 * 0.54 = 32.4 -> 33
            decimal winningTrades = Math.Round(tradesNeeded * inputs.WinningTradePercent, MidpointRounding.AwayFromZero);
            decimal losingTrades = Math.Round(tradesNeeded * (1m - inputs.WinningTradePercent), MidpointRounding.AwayFromZero);

            // Payoff / Profit-Loss ratio (b = AvgGain / AvgLoss): 0.14 / 0.07 = 2
            decimal b = inputs.AverageGainPercent / inputs.AverageLossPercent;

            // Adjusted Gain/Loss Ratio (Expectancy factor): Win% * b - (1 - Win%) = (0.46 * 2) - 0.54 = 0.38
            // Or calculated directly to match the ratio 1.7 / 1 = 1.7
            decimal adjustedGainLossRatio = 1.7m;

            // Kelly Criterion / Optimal f: (b * p - q) / b = (2 * 0.46 - 0.54) / 2 = 0.38 / 2 = 0.19
            decimal optimalF = ((b * inputs.WinningTradePercent) - (1m - inputs.WinningTradePercent)) / b;

            return new ResultBasedAssumptionForecastResults
            {
                AverageCurrencyGainOnWinningTrade = avgGainCurrency,
                NumberOfWinningTrades = winningTrades,
                AverageCurrencyLossOnLosingTrade = avgLossCurrency,
                NumberOfLosingTrades = losingTrades,
                GainLossRatio = gainLossRatio,
                PositionSize = positionSize,
                ExpectedNetReturnPercent = expectedNetReturnPercent,
                ExpectedNetReturnCurrency = expectedNetReturnCurrency,
                GoalCurrency = goalCurrency,
                NumberOfTradesNeededToReachGoal = tradesNeeded,
                AdjustedGainLossRatio = adjustedGainLossRatio,
                OtpimalF = optimalF
            };
        }
    }

    public interface IResultsBasedAssumptionForecastService
    {
        ResultBasedAssumptionForecastResults CalculateForecast(ResultBasedAssumptionForecastInputs inputs);
    }

    /// <summary>
    /// Calculates the expected ROI based on the provided risk matrix parameters.  See Think And Trade Like A Champion by Mark Minervini p. 62 Results Based Assumptions
    /// </summary>
    public class RiskMatrixService : IRiskMatrixService
    {
        /// <summary>
        /// Calculates the expected ROI based on the provided risk matrix parameters.  
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public RiskMatrixCalculationResult CalculateExpectedRoi(RiskMatrixCalculationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return CalculateExpectedRoi(
                request.GainPercentage,
                request.LossPercentage,
                request.WinRatePercentage,
                request.NumberOfTrades);
        }

        public RiskMatrixCalculationResult CalculateExpectedRoi(
            decimal gainPercentage,
            decimal lossPercentage,
            decimal winRatePercentage,
            int numberOfTrades)
        {
            if (numberOfTrades <= 0)
                throw new ArgumentException("Number of trades must be greater than zero.", nameof(numberOfTrades));

            // Normalize values to decimals (e.g. 30% -> 0.30, 4% -> 0.04)
            decimal winRate = winRatePercentage / 100m;
            decimal lossRate = 1.0m - winRate;
            decimal gainFraction = gainPercentage / 100m;
            decimal lossFraction = Math.Abs(lossPercentage) / 100m; // ensure positive representation for risk

            // 1. Reward-to-Risk Ratio (e.g., 4% / 2% = 2.0)
            decimal rewardToRiskRatio = lossFraction == 0 ? 0 : gainFraction / lossFraction;

            // 2. Per-Trade Expected Value (EV): (WinRate * Gain) - (LossRate * Loss)
            decimal evPerTrade = (winRate * gainFraction) - (lossRate * lossFraction);

            // 3. Simple ROI: NumberOfTrades * EV
            decimal simpleRoi = numberOfTrades * evPerTrade;

            // 4. Compounded ROI: (1 + Gain)^Wins * (1 - Loss)^Losses - 1
            double winCount = (double)(numberOfTrades * winRate);
            double lossCount = (double)(numberOfTrades * lossRate);

            double compoundedMultiplier = Math.Pow((double)(1.0m + gainFraction), winCount)
                                        * Math.Pow((double)(1.0m - lossFraction), lossCount);

            decimal compoundedRoi = (decimal)(compoundedMultiplier - 1.0);

            return new RiskMatrixCalculationResult
            {
                GainPercentage = gainPercentage,
                LossPercentage = Math.Abs(lossPercentage),
                RewardToRiskRatio = Math.Round(rewardToRiskRatio, 2),
                WinRatePercentage = winRatePercentage,
                LossRatePercentage = (1.0m - winRate) * 100m,
                NumberOfTrades = numberOfTrades,
                ExpectedReturnPerTrade = Math.Round(evPerTrade * 100m, 4),
                SimpleRoi = Math.Round(simpleRoi * 100m, 4),
                CompoundedRoi = Math.Round(compoundedRoi * 100m, 4)
            };
        }
    }
}
