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
}
