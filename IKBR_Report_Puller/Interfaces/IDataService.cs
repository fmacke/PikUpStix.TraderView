using System.Xml.Linq;

namespace IKBR_Report_Puller.Interfaces
{
    public interface IDataService
    {
        void InsertOpenPositions(XDocument reportXml);
        void InsertTradeExecutions(XDocument reportXml);
        void InsertTodayExecutions(XDocument reportXml);
        void UpsertTimeSeriesData(string instrumentName, string listingExchange, string securityIdentifier, string provider, string dataName, string dataSource, string format, string frequency, string currency, DateTime date, double openPrice, double closePrice, double lowPrice, double highPrice, double volume);
        string ConnectionString { get; }
        List<(string securityID, string listingExchange, string symbol)> GetOpenPositionInstrumentNames(XDocument reportXml);
    }
}
