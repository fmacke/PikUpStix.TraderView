using IKBR_Report_Puller.Domain;
using System.Xml.Linq;

namespace IKBR_Report_Puller.Interfaces
{
    public interface IExcelReportService
    {
        void CreateReport(XDocument reportXml, string outputFilePath);
    }
    public interface ITradeHistoryReportService
    {
        void CreateTradeHistoryReport(List<TradeExecution> tradeExecutions);
        public List<HistoricalTrade> TradeHistory { get; set; }
        public List<HistoricalTrade> TradeHistoryAggregated { get; set; }
    }
}
