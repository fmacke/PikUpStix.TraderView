namespace traderview.Server.DTOs
{
    public class CurrentPerformanceResultDto
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
        public decimal AverageWin { get; set; }              // Average winning trade amount (£)
        public decimal AverageLoss { get; set; }             // Average losing trade amount (£)
        public decimal LargestWin { get; set; }              // Largest single win amount (£)
        public int LargestWinPositionId { get; set; }        // Position ID of largest win
        public decimal LargestLoss { get; set; }             // Largest single loss amount (£)
        public int LargestLossPositionId { get; set; }       // Position ID of largest loss
        public double AverageDaysHeld { get; set; }          // Average days all positions are held
        public double AverageDaysHeldGains { get; set; }     // Average days winning positions are held
        public double AverageDaysHeldLosses { get; set; }    // Average days losing positions are held
    }
}
