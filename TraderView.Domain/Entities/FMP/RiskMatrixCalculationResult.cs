namespace TraderView.Domain.Entities.FMP
{
    public class RiskMatrixCalculationRequest
    {
        public decimal GainPercentage { get; set; }          // e.g., 4.0 for +4%
        public decimal LossPercentage { get; set; }          // e.g., 2.0 for -2% (risk)
        public decimal WinRatePercentage { get; set; }       // e.g., 30.0 for 30%
        public int NumberOfTrades { get; set; } = 10;        // e.g., 10
    }

    public class RiskMatrixCalculationResult
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
}
