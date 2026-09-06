using TraderView.Domain.Entities;
using TraderView.Domain.Entities.FMP;

namespace TraderView.Application.Interfaces.Services
{
    public interface ITradeHistoryReportService
    {
        void CreateTradeHistoryReport(List<TradeExecution> tradeExecutions);
        List<HistoricalTrade> TradeHistory { get; set; }
        List<HistoricalTrade> TradeHistoryAggregated { get; set; }
        RiskMatrixCalculationRequest RiskMatrixCalculationRequest { get; set; }
    }
}
