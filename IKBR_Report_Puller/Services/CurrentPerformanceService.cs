using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;
using TraderView.Domain.Entities.FMP;

namespace TraderView.Application.Services
{
    /// <summary>
    /// Calculates the expected ROI based on the provided risk matrix parameters.  See Think And Trade Like A Champion by Mark Minervini p. 62 Results Based Assumptions
    /// </summary>
    public class CurrentPerformanceService : ICurrentPerformanceService
    {
        /// <summary>
        /// Calculates the expected ROI based on the provided risk matrix parameters.  
        /// </summary>
        /// <param name="request"></param>
        /// <param name="positions">Optional list of closed positions to calculate additional metrics</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<CurrentPerformanceResult> CalculateExpectedRoi(RiskMatrixCalculationRequest request, IReadOnlyList<Position>? positions = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return await Task.Run(() => CalculateExpectedRoi(request.GainPercentage, request.LossPercentage, request.WinRatePercentage, request.NumberOfTrades, positions));
        }

        public async Task<CurrentPerformanceResult> CalculateExpectedRoi(decimal gainPercentage, decimal lossPercentage, decimal winRatePercentage, int numberOfTrades, IReadOnlyList<Position>? positions = null)
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

            var result = new CurrentPerformanceResult
            {
                GainPercentage = Math.Round(gainPercentage, 1),
                LossPercentage = Math.Round(lossPercentage, 1),
                RewardToRiskRatio = Math.Round(rewardToRiskRatio, 1),
                WinRatePercentage = Math.Round(winRatePercentage, 1),
                LossRatePercentage = Math.Round((1.0m - winRate) * 100m, 1),
                NumberOfTrades = numberOfTrades,
                ExpectedReturnPerTrade = Math.Round(evPerTrade * 100m, 2),
                SimpleRoi = Math.Round(simpleRoi * 100m, 4),
                CompoundedRoi = Math.Round(compoundedRoi * 100m, 2)
            };

            // Calculate position-based metrics if positions are provided
            if (positions != null && positions.Count > 0)
            {
                var positionMetrics = CalculatePositionMetrics(positions);
                result.AverageWin = positionMetrics.AverageWin;
                result.AverageLoss = positionMetrics.AverageLoss;
                result.LargestWin = positionMetrics.LargestWin;
                result.LargestWinPositionId = positionMetrics.LargestWinPositionId;
                result.LargestLoss = positionMetrics.LargestLoss;
                result.LargestLossPositionId = positionMetrics.LargestLossPositionId;
                result.AverageDaysHeld = positionMetrics.AverageDaysHeld;
                result.AverageDaysHeldGains = positionMetrics.AverageDaysHeldGains;
                result.AverageDaysHeldLosses = positionMetrics.AverageDaysHeldLosses;
            }

            return await Task.Run(() => result);
        }

        /// <summary>
        /// Calculates position-based metrics from actual closed positions
        /// </summary>
        private (decimal AverageWin, decimal AverageLoss, decimal LargestWin, int LargestWinPositionId, 
                  decimal LargestLoss, int LargestLossPositionId, double AverageDaysHeld, 
                  double AverageDaysHeldGains, double AverageDaysHeldLosses) CalculatePositionMetrics(IReadOnlyList<Position> positions)
        {
            // Filter to only closed positions with valid CloseDate and NetProfit
            var closedPositions = positions
                .Where(p => p.CloseDate.HasValue && p.NetProfit.HasValue)
                .ToList();

            if (closedPositions.Count == 0)
            {
                return (0, 0, 0, 0, 0, 0, 0, 0, 0);
            }

            var winningPositions = closedPositions.Where(p => p.NetProfit > 0).ToList();
            var losingPositions = closedPositions.Where(p => p.NetProfit < 0).ToList();

            // Calculate average win and loss
            decimal averageWin = winningPositions.Count > 0 
                ? Math.Round(winningPositions.Average(p => p.NetProfit ?? 0), 2)
                : 0;

            decimal averageLoss = losingPositions.Count > 0 
                ? Math.Round(losingPositions.Average(p => Math.Abs(p.NetProfit ?? 0)), 2)
                : 0;

            // Find largest win and loss
            var largestWinPosition = winningPositions.OrderByDescending(p => p.NetProfit).FirstOrDefault();
            decimal largestWin = largestWinPosition?.NetProfit ?? 0;
            int largestWinPositionId = largestWinPosition?.Id ?? 0;

            var largestLossPosition = losingPositions.OrderBy(p => p.NetProfit).FirstOrDefault(); // Most negative
            decimal largestLoss = largestLossPosition != null ? Math.Abs(largestLossPosition.NetProfit ?? 0) : 0;
            int largestLossPositionId = largestLossPosition?.Id ?? 0;

            // Calculate average days held
            double averageDaysHeld = closedPositions.Average(p => (p.CloseDate!.Value - p.OpenDate).TotalDays);

            double averageDaysHeldGains = winningPositions.Count > 0 
                ? winningPositions.Average(p => (p.CloseDate!.Value - p.OpenDate).TotalDays)
                : 0;

            double averageDaysHeldLosses = losingPositions.Count > 0 
                ? losingPositions.Average(p => (p.CloseDate!.Value - p.OpenDate).TotalDays)
                : 0;

            return (
                averageWin,
                averageLoss,
                Math.Round(largestWin, 2),
                largestWinPositionId,
                largestLoss,
                largestLossPositionId,
                Math.Round(averageDaysHeld, 1),
                Math.Round(averageDaysHeldGains, 1),
                Math.Round(averageDaysHeldLosses, 1)
            );
        }
    }
}
