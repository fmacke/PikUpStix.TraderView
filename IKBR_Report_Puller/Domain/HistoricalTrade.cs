namespace IKBR_Report_Puller.Domain
{
    public class HistoricalTrade : TradeBase
    {
        public decimal MarketValue => ClosePrice * (decimal)Math.Sqrt((double)Quantity * (double)Quantity);
        public decimal RealizedPnL => MarketValue - TotalCost;
        public decimal RealizedPnLPercentage => TotalCost != 0 ? (RealizedPnL / TotalCost) * 100 : 0;
        public long OpenIbOrderID { get; set; }
        public long CloseIbOrderID { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal IBCommission { get; set; } = 0;
        public string IBCommissionCurrency { get; set; }
        public decimal TotalCost => AveragePrice * (decimal)Math.Sqrt((double)Quantity * (double)Quantity);
        public DateTime TradeOpened{ get; set; }
        public DateTime TradeClosed { get; set; }
    }
}
