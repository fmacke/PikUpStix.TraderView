using IKBR_Report_Puller.Domain;

namespace PikUpStix.TraderView.Domain
{
    public class Position 
    {        
        public int Id { get; set; }
        public DateTime OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int InstrumentId { get; set; }
        public List<TradeExecution> TradeExecutions { get; set; } = new List<TradeExecution>();
        public Instrument Instrument { get; set; }
    }
}
