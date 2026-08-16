using System.Threading.Tasks;
using System.Xml.Linq;

namespace TraderView.Application.Interfaces.Services
{
    public interface IReportFetchingService
    {
        Task<XDocument> FetchMainReportAsync(int maxRetries, int delayInSeconds);
        Task<XDocument> FetchTodayReportAsync(int maxRetries, int delayInSeconds);
        
    }
}
