using IBApi;
using System.Xml.Linq;

namespace IKBR_Report_Puller.Interfaces
{
    public interface IChartDataService
    {
        Task<List<Bar>> GetHistoricalDataAsync(string symbol, string assetCategory, string currency, string listingExchange);
        Task<bool> ConnectAsync(string host, int port, int clientId);
    }
}
