namespace IKBR_Report_Puller.Domain
{
    /// <summary>
    /// Represents an aggregated summary of trade executions for a single order
    /// </summary>
    public class TradeSummary
    {
        public int InstrumentId { get; set; }
        public int PositionId { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; }
        public DateTime ExitDate { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal ExitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal Pnl { get; set; }
        public string BuySell { get; set; } = string.Empty;
    }
}
