namespace TraderView.Domain.Entities.FMP
{
    public class RiskMatrixCalculationRequest
    {
        private List<HistoricalTrade> _trades { get; set; } = new List<HistoricalTrade>();
        public RiskMatrixCalculationRequest() { }
        public RiskMatrixCalculationRequest(List<HistoricalTrade> trades)
        {
            _trades = trades;
            NumberOfTrades = trades.Count;
        }
        public decimal GainPercentage { get => CalculateGainPercentage(); }          // e.g., 4.0 for +4%
        public decimal LossPercentage { get => CalculateLossPercentage(); }          // e.g., 2.0 for -2% (risk)
        public decimal WinRatePercentage { get => CalculateWinRatePercentage(); }       // e.g., 30.0 for 30%
        public int NumberOfTrades { get; set; } = 10;        // e.g., 10

        private decimal CalculateGainPercentage()
        {
            if (_trades.Count == 0) return 0;
            decimal totalGain = (decimal)_trades.Where(t => t.ClosePrice > t.TradePrice).Sum(t => (t.ClosePrice - t.TradePrice) * t.Quantity);
            decimal totalInvestment = (decimal)_trades.Sum(t => t.TradePrice * t.Quantity);
            return totalInvestment == 0 ? 0 : (totalGain / totalInvestment) * 100;
        }
        private decimal CalculateLossPercentage()
        {
            if (_trades.Count == 0) return 0;
            decimal totalLoss = (decimal)_trades.Where(t => t.ClosePrice < t.TradePrice).Sum(t => (t.TradePrice - t.ClosePrice) * t.Quantity);
            decimal totalInvestment = (decimal)_trades.Sum(t => t.TradePrice * t.Quantity);
            return totalInvestment == 0 ? 0 : (totalLoss / totalInvestment) * 100;
        }
        private decimal CalculateWinRatePercentage()
        {
            if (_trades.Count == 0) return 0;
            int winningTrades = _trades.Count(t => t.ClosePrice > t.TradePrice);
            return (decimal)winningTrades / _trades.Count * 100;
        }
    }

    public class CurrentPerformanceResult
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
