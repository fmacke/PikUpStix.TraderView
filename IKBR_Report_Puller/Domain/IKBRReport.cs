using System;
using System.Collections.Generic;

namespace IKBR_Report_Puller.Domain
{
    public class IKBRReport
    {
        public DateTime WhenGenerated { get; set; }
        public string AccountId { get; set; }
        public List<TradeExecution> Trades { get; set; } = new List<TradeExecution>();
        public List<OpenPosition> OpenPositions { get; set; } = new List<OpenPosition>();
        public List<TradeExecution> TradeConfirms { get; set; } = new List<TradeExecution>();
    }
}
