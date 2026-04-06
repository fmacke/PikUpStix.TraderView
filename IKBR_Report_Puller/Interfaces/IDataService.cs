using System.Xml.Linq;
using IKBR_Report_Puller.Services;
using IKBR_Report_Puller.Domain;

namespace IKBR_Report_Puller.Interfaces
{
    public interface IDataService
    {
        void InsertOpenPositions(IKBRReport report);
        void InsertTradeExecutions(IKBRReport report);
        void InsertTodayExecutions(IKBRReport report);
        void UpsertHistoricalData(string instrumentId, List<Bar> bars);
        List<(DateTime startDate, DateTime endDate)> GetMissingDateRanges(int instrumentId, DateTime startDate, DateTime endDate);
         string ConnectionString { get; }   
        List<TradeExecution> GetTradeExecutions();
    }
}
