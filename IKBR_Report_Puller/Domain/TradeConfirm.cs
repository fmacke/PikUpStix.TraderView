using System;

namespace IKBR_Report_Puller.Domain
{
    public class TradeConfirm
    {
        public string ExecID { get; set; }
        public string Symbol { get; set; }
        public string TradeDate { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Price { get; set; }
    }
}
