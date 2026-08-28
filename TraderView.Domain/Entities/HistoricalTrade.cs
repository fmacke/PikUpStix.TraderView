namespace TraderView.Domain.Entities
{
    public class HistoricalTrade : TradeExecution
    {
        public decimal MarketValue => ClosePrice * (decimal)Math.Sqrt((double)Quantity * (double)Quantity);
        public decimal RealizedPnL => MarketValue - TotalCost;
        public decimal RealizedPnLPercentage => TotalCost != 0 ? (RealizedPnL / TotalCost) * 100 : 0;
        public long OpenIbOrderID { get; set; }
        public long CloseIbOrderID { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal IbCommission { get; set; } = 0;
        public string IbCommissionCurrency { get; set; }
        public decimal TotalCost => Convert.ToDecimal(TradePrice) * (decimal)Math.Sqrt((double)Convert.ToDecimal(Quantity) * (double)Convert.ToDecimal(Quantity)) - IbCommission;
        public DateTime TradeOpened{ get; set; }
        public DateTime TradeClosed { get; set; }
        public int InstrumentId { get; set; }
    }
}
 