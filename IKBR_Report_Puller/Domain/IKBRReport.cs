namespace PikUpStix.TraderView.Domain
{
    public class IKBRReport
    {
        public DateTime WhenGenerated { get; set; }
        public string AccountId { get; set; }
        public List<TradeExecution> Trades { get; set; } = new List<TradeExecution>();
        public List<Position> OpenPositions { get; set; } = new List<Position>();
        public List<TradeConfirm> TradeConfirms { get; set; } = new List<TradeConfirm>();
    }
}
