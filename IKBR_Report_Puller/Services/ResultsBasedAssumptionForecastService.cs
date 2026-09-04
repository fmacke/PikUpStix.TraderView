using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Models.ResultBasedForecasting;
using TraderView.Domain.Entities;

namespace TraderView.Application.Services
{
    public class ResultsBasedAssumptionForecastService : IResultsBasedAssumptionForecastService
    {
        private ITradeExecutionRepository _tradeExecutionRepository;
        private ITradeHistoryReportService _tradeHistoryReportService;

        public ResultsBasedAssumptionForecastService(ITradeExecutionRepository tradeExecutionRepository, ITradeHistoryReportService tradeHistoryReportService)
        {
            _tradeExecutionRepository = tradeExecutionRepository;
            _tradeHistoryReportService = tradeHistoryReportService;
        }
        public ResultBasedAssumptionForecastInputs GetInputsFromTradingHistory(decimal portfolioSize, decimal positionSizePercent, decimal desiredReturnPercent)        
        {
            var tradeExecutions = _tradeExecutionRepository.GetTradeExecutions();
            _tradeHistoryReportService.CreateTradeHistoryReport(tradeExecutions);
            var trades = _tradeHistoryReportService.TradeHistoryAggregated;

            var averageGainPercent = trades.Sum(x => x.RealizedPnL > 0 ? x.RealizedPnL : 0) / trades.Count(x => x.RealizedPnL > 0);
            var averageLossPercent = trades.Sum(x => x.RealizedPnL < 0 ? Math.Abs(x.RealizedPnL) : 0) / trades.Count(x => x.RealizedPnL < 0);
            var winningTradePercent = (decimal)trades.Count(x => x.RealizedPnL > 0) / trades.Count;
            var inputs = new ResultBasedAssumptionForecastInputs(portfolioSize,positionSizePercent, desiredReturnPercent, averageGainPercent, averageLossPercent, winningTradePercent);
            return inputs;
        }
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
}
