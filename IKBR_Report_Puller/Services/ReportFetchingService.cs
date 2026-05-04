using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using IKBR_Report_Puller.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Linq;
using IBApi; // Ensure TwsClient is referenced
using System.Net.Http;

namespace IKBR_Report_Puller.Services
{
    public class ReportFetchingService : IReportFetchingService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        // Config values for reports (Path B)
        private readonly string _token;
        private readonly string _baseUrl;
        private readonly string _mainQueryId;
        private readonly string _todayQueryId;

        public ReportFetchingService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;

            _token = _config["IBKR:Token"];
            _baseUrl = _config["IBKR:BaseUrl"];
            _mainQueryId = _config["IBKR:QueryId"];
            _todayQueryId = _config["IBKR:QueryTodayExecutionsId"];
        }



        public async Task<XDocument> FetchMainReportAsync(int maxRetries, int delayInSeconds)
        {
            Console.WriteLine("Fetching main report...");
            var client = _httpClientFactory.CreateClient("IKBR");
            var service = new IKBRReportServiceBase(_token, _mainQueryId, _baseUrl, client);
            return await service.FetchReportAsync(maxRetries, delayInSeconds);
        }

        public async Task<XDocument> FetchTodayReportAsync(int maxRetries, int delayInSeconds)
        {
            Console.WriteLine("\nFetching 'Today' report...");
            var client = _httpClientFactory.CreateClient("IKBR");
            var service = new IKBRReportServiceBase(_token, _todayQueryId, _baseUrl, client);
            return await service.FetchReportAsync(maxRetries, delayInSeconds);
        }
    }
}