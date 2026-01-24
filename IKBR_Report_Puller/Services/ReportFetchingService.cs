using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using IKBR_Report_Puller.Interfaces;
using Microsoft.Extensions.Configuration;

namespace IKBR_Report_Puller.Services
{
    public class ReportFetchingService : IReportFetchingService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _client;
        private readonly string _token;
        private readonly string _baseUrl;
        private readonly string _mainQueryId;
        private readonly string _todayQueryId;

        public ReportFetchingService(IConfiguration config, HttpClient client)
        {
            _config = config;
            _client = client;
            _token = _config["IBKR:Token"];
            _baseUrl = _config["IBKR:BaseUrl"];
            _mainQueryId = _config["IBKR:QueryId"];
            _todayQueryId = _config["IBKR:QueryTodayExecutionsId"];
        }

        public async Task<XDocument> FetchMainReportAsync(int maxRetries, int delayInSeconds)
        {
            Console.WriteLine("Fetching main report...");
            var service = new IKBRReportServiceBase(_token, _mainQueryId, _baseUrl, _client);
            return await service.FetchReportAsync(maxRetries, delayInSeconds);
        }

        public async Task<XDocument> FetchTodayReportAsync(int maxRetries, int delayInSeconds)
        {
            Console.WriteLine("\nFetching 'Today' report...");
            var service = new IKBRReportServiceBase(_token, _todayQueryId, _baseUrl, _client);
            return await service.FetchReportAsync(maxRetries, delayInSeconds);
        }
    }
}
