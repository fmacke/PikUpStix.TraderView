using IKBR_Report_Puller.Domain;

namespace PikUpStix.TraderView.Interfaces
{
    public interface ITradeHistoryReportService
    {
        void CreateTradeHistoryReport(List<Trade> tradeExecutions);
        public List<HistoricalTrade> TradeHistory { get; set; }
        public List<HistoricalTrade> TradeHistoryAggregated { get; set; }
    }
}
