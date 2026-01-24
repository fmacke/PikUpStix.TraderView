using System.Xml.Linq;

namespace IKBR_Report_Puller.Interfaces
{
    public interface IDataService
    {
        void InsertOpenPositions(XDocument reportXml);
        void InsertTradeExecutions(XDocument reportXml);
        void InsertTodayExecutions(XDocument reportXml);
        string ConnectionString { get; }
    }
}
