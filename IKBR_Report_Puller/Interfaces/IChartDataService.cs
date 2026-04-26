using IBApi;
using System.Xml.Linq;

namespace IKBR_Report_Puller.Interfaces
{
    public interface IChartDataService
    {
        Task<List<Bar>> GetHistoricalDataAsync(string conid, string listingExchange, DateTime from, DateTime to, string symbol = null, string contractUnitType = null);
        Task<bool> ConnectAsync(string host, int port, int clientId);
    }
}
