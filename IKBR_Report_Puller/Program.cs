using System;
using System.Net.Http;
using System.Threading.Tasks;
using IKBR_Report_Puller.Interfaces;
using IKBR_Report_Puller.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IKBR_Report_Puller
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        config.AddUserSecrets<Program>();
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<HttpClient>();
                    services.AddSingleton<IDataService, DataService>();
                    services.AddSingleton<IExcelReportService, ExcelReportService>();
                    services.AddSingleton<IReportFetchingService, ReportFetchingService>();
                    services.AddSingleton<ITradeHistoryReportService, TradeHistoryService>();
                    services.AddSingleton<IChartDataService, ChartDataService>();
                    services.AddSingleton<Application>();
                })
                .Build();

            var app = host.Services.GetRequiredService<Application>();
            await app.RunAsync();
        }
    }
}
