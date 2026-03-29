using IBApi;
using System.Xml.Linq;

namespace IKBR_Report_Puller.Interfaces
{
    public interface IChartService
    {
        Task<List<Bar>> GetHistoricalDataAsync(string symbol);
        Task<bool> ConnectAsync(string host, int port, int clientId);
    }
}
