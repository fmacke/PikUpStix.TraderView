using System;
using System.Net.Http;
using System.Threading.Tasks;
using IKBR_Report_Puller.Interfaces;
using IKBR_Report_Puller.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PikUpStix.TraderView.Interfaces;

namespace IKBR_Report_Puller.Console
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
                    // Register HttpClient factory to avoid DNS and socket exhaustion issues
                    services.AddHttpClient("IKBR", client =>
                    {
                        client.Timeout = TimeSpan.FromMinutes(5);
                    })
                    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                    {
                        PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1)
                    });

                    // Register repositories (repositories should be scoped or transient, but using singleton for console app simplicity)
                    services.AddSingleton<IInstrumentRepository>(provider =>
                    {
                        var config = provider.GetRequiredService<IConfiguration>();
                        var connectionString = BuildConnectionString(config);
                        return new Data.Repositories.InstrumentRepository(connectionString);
                    });

                    services.AddSingleton<ITradeExecutionRepository>(provider =>
                    {
                        var config = provider.GetRequiredService<IConfiguration>();
                        var instrumentRepo = provider.GetRequiredService<IInstrumentRepository>();
                        var connectionString = BuildConnectionString(config);
                        return new Data.Repositories.TradeExecutionRepository(connectionString, instrumentRepo);
                    });

                    services.AddSingleton<IHistoricalDataRepository>(provider =>
                    {
                        var config = provider.GetRequiredService<IConfiguration>();
                        var connectionString = BuildConnectionString(config);
                        return new Data.Repositories.HistoricalDataRepository(connectionString);
                    });

                    services.AddSingleton<IOpenPositionRepository>(provider =>
                    {
                        var config = provider.GetRequiredService<IConfiguration>();
                        var connectionString = BuildConnectionString(config);
                        return new Data.Repositories.OpenPositionRepository(connectionString);
                    });

                    // Register services
                    services.AddSingleton<IExcelReportService, ExcelReportService>();
                    services.AddSingleton<IReportFetchingService, ReportFetchingService>();
                    services.AddSingleton<ITradeHistoryReportService, TradeHistoryService>();
                    services.AddSingleton<IChartDataService, ChartDataService>();
                    services.AddSingleton<IHistoricalDataService, HistoricalDataService>();
                    services.AddSingleton<Application>();
                })
                .Build();

            var app = host.Services.GetRequiredService<Application>();
            await app.RunAsync();
        }

        // Helper method to build connection string
        static string BuildConnectionString(IConfiguration config)
        {
            var dbUser = config["Database:User"];
            var dbPassword = config["Database:Password"];
            var dbHost = config["Database:Host"];
            var dbName = config["Database:DbName"];
            return $"Server={dbHost};Database={dbName};User ID={dbUser};Password={dbPassword};TrustServerCertificate=True;";
        }
    }
}
