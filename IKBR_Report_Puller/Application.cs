using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using IKBR_Report_Puller.Interfaces;
using Microsoft.Extensions.Configuration;

namespace IKBR_Report_Puller
{
    public class Application
    {
        private readonly IReportFetchingService _reportFetchingService;
        private readonly IDataService _dataService;
        private readonly IExcelReportService _excelReportService;
        private readonly IConfiguration _config;
        private readonly ITimeSeriesService _timeSeriesService;

        public Application(
            IReportFetchingService reportFetchingService,
            IDataService dataService,
            IExcelReportService excelReportService,
            IConfiguration config,
            ITimeSeriesService timeSeriesService)
        {
            _reportFetchingService = reportFetchingService;
            _dataService = dataService;
            _excelReportService = excelReportService;
            _config = config;
            _timeSeriesService = timeSeriesService;
        }

        public async Task RunAsync()
        {
            try
            {
                //var outputFilePath = _config["IBKR:OutputFilePath"];
                //const int maxRetries = 10;
                //const int delayInSeconds = 15;

                //// Fetch and process main report
                //XDocument mainReportXml = await _reportFetchingService.FetchMainReportAsync(maxRetries, delayInSeconds);
                //string mainReportFilePath = outputFilePath.Replace("[FILE_NAME]", "TraderSyncAccess.xml");
                //mainReportXml.Save(mainReportFilePath);
                //Console.WriteLine($"Successfully saved main report to {mainReportFilePath}");

                //_dataService.InsertTradeExecutions(mainReportXml);
                //_dataService.InsertOpenPositions(mainReportXml);
                //_excelReportService.CreateOpenPositionsReport(mainReportXml, outputFilePath);

                //// Fetch and process today's report
                //XDocument todayReportXml = await _reportFetchingService.FetchTodayReportAsync(maxRetries, delayInSeconds);
                //string todayReportFilePath = outputFilePath.Replace("[FILE_NAME]", "TraderSyncAccess_today.xml");
                //todayReportXml.Save(todayReportFilePath);
                //Console.WriteLine($"Successfully saved 'Today' report to {todayReportFilePath}");

                //_dataService.InsertTodayExecutions(todayReportXml);

                // Fetch time series data for NYSE BAP instrument
                string ticker = "BAP";
                DateTime startDate = DateTime.UtcNow.AddMonths(-1);
                DateTime endDate = DateTime.UtcNow;
                string period = "1d";

                string timeSeriesData = await _timeSeriesService.GetTimeSeriesDataAsync(ticker, startDate, endDate, period);
                Console.WriteLine("Time Series Data for NYSE BAP:");
                Console.WriteLine(timeSeriesData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
            }
        }
    }
}
