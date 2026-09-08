namespace traderview.Server.DTOs
{
    public class DesiredPerformanceResultsDto
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
        public decimal NumberOfTradesNeededToReachGoal { get; set; }
        public decimal AdjustedGainLossRatio { get; set; }
        public decimal OtpimalF { get; set; }
    }
}
