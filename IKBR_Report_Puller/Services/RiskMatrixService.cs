using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace TraderView.Application.Services
{
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
