namespace IKBR_Report_Puller.Domain
{
    public class HistoricalTrade : TradeBase
    {
        public decimal MarketValue => ClosePrice * (decimal)Math.Sqrt((double)Quantity * (double)Quantity);
        public decimal RealizedPnL => MarketValue - TotalCost;
        public long OpenIbOrderID { get; set; }
        public long CloseIbOrderID { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal TotalCost => AveragePrice * (decimal)Math.Sqrt((double)Quantity * (double)Quantity);
        public DateTime TradeOpened{ get; set; }
        public DateTime TradeClosed { get; set; }
    }
}
