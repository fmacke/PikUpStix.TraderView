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
        void UpsertTimeSeriesData(string instrumentName, string listingExchange, string securityIdentifier, string provider, string dataName, string dataSource, string format, string frequency, string currency, DateTime date, double openPrice, double closePrice, double lowPrice, double highPrice, double volume);
        void InsertChartData(string instrumentId, List<Bar> bars);
        string ConnectionString { get; }
        List<(string securityID, string listingExchange, string symbol)> GetOpenPositionInstrumentNames(IKBRReport report);
        List<TradeExecution> GetTradeExecutions();
    }
}
