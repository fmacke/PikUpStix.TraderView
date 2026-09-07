namespace traderview.Server.DTOs
{
    public class RiskMatrixCalculationResultDto
    {
            public decimal GainPercentage { get; set; }
            public decimal LossPercentage { get; set; }
            public decimal RewardToRiskRatio { get; set; }
            public decimal WinRatePercentage { get; set; }
            public decimal LossRatePercentage { get; set; }
            public int NumberOfTrades { get; set; }
            public decimal ExpectedReturnPerTrade { get; set; }  // EV % per trade
            public decimal SimpleRoi { get; set; }               // Non-compounded %
            public decimal CompoundedRoi { get; set; }           // Compounded %
    }
    public class ResultBasedAssumptionForecastResultsDto
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
