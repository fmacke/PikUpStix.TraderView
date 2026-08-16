using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface ITradeHistoryReportService
    {
        void CreateTradeHistoryReport(List<TradeExecution> tradeExecutions);
        public List<HistoricalTrade> TradeHistory { get; set; }
        public List<HistoricalTrade> TradeHistoryAggregated { get; set; }
    }
}
