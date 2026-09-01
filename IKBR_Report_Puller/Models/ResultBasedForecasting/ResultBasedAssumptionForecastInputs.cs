namespace TraderView.Application.Models.ResultBasedForecasting
{
    public class ResultBasedAssumptionForecastInputs
    {
        public ResultBasedAssumptionForecastInputs(decimal portfolioSizeCurrency, decimal positionSizePercent, decimal desiredReturnPercent, decimal averageGainPercent, decimal averageLossPercent, decimal winningTradePercent)
        {
            PortfolioSizeCurrency = portfolioSizeCurrency;
            PositionSizePercent = positionSizePercent;
            DesiredReturnPercent = desiredReturnPercent;
            AverageGainPercent = averageGainPercent;
            AverageLossPercent = averageLossPercent;
            WinningTradePercent = winningTradePercent;
        }

        public decimal PortfolioSizeCurrency { get; private set; }
        public decimal PositionSizePercent { get; private set; }
        public decimal DesiredReturnPercent { get; private set; }
        public decimal AverageGainPercent { get; private set; }
        public decimal AverageLossPercent { get; private set; }
        public decimal WinningTradePercent { get; private set; }

    }
    public class ResultBasedAssumptionForecastResults
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
    }
}