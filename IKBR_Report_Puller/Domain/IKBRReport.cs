using System;
using System.Collections.Generic;

namespace IKBR_Report_Puller.Domain
{
    public class IKBRReport
    {
        public DateTime WhenGenerated { get; set; }
        public string AccountId { get; set; }
        public List<Trade> Trades { get; set; } = new List<Trade>();
        public List<OpenPosition> OpenPositions { get; set; } = new List<OpenPosition>();
        public List<Trade> TradeConfirms { get; set; } = new List<Trade>();
    }
}
