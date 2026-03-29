using System.Threading.Tasks;
using System.Xml.Linq;

namespace IKBR_Report_Puller.Interfaces
{
    public interface IReportFetchingService
    {
        Task<XDocument> FetchMainReportAsync(int maxRetries, int delayInSeconds);
        Task<XDocument> FetchTodayReportAsync(int maxRetries, int delayInSeconds);
        
    }
}
