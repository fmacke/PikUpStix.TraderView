using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PikUpStix.TraderView.Services;
using PikUpStix.TraderView.Services.MarketData;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Services;
using TraderView.Infrastructure.Repositories;

namespace TraderView.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

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
                        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
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
                        return new InstrumentRepository(connectionString);
                    });

                    services.AddSingleton<IPositionRepository>(provider =>
                    {
                        var config = provider.GetRequiredService<IConfiguration>();
                        var instrumentRepo = provider.GetRequiredService<IInstrumentRepository>();
                        var connectionString = BuildConnectionString(config);
                        return new PositionRepository(connectionString, instrumentRepo);
                    });

                    services.AddSingleton<ITradeExecutionRepository>(provider =>
                    {
                        var config = provider.GetRequiredService<IConfiguration>();
                        var instrumentRepo = provider.GetRequiredService<IInstrumentRepository>();
                        var connectionString = BuildConnectionString(config);
                        return new TradeExecutionRepository(connectionString, instrumentRepo);
                    });

                    services.AddSingleton<IHistoricalDataRepository>(provider =>
                    {
                        var config = provider.GetRequiredService<IConfiguration>();
                        var connectionString = BuildConnectionString(config);
                        return new HistoricalDataRepository(connectionString);
                    });

                    services.AddSingleton<IEconomicCalendarRepository>(provider =>
                    {
                        var config = provider.GetRequiredService<IConfiguration>();
                        var connectionString = BuildConnectionString(config);
                        return new EconomicCalendarRepository(connectionString);
                    });

                    services.AddSingleton<ICanSlimCandidateRepository>(provider =>
                    {
                        var config = provider.GetRequiredService<IConfiguration>();
                        var connectionString = BuildConnectionString(config);
                        return new CanSlimCandidateRepository(connectionString);
                    });

                    // Register both market data services
                    services.AddSingleton<FinancialModellingPrepService>(provider =>
                    {
                        var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("IKBR");
                        var repository = provider.GetRequiredService<IEconomicCalendarRepository>();
                        var historicalDataRepository = provider.GetRequiredService<IHistoricalDataRepository>();
                        var instrumentRepository = provider.GetRequiredService<IInstrumentRepository>();
                        var config = provider.GetRequiredService<IConfiguration>();

                        var apiKey = config["FinancialModelingPrep:ApiKey"];
                        var baseUrl = config["FinancialModelingPrep:BaseUrl"];
                        var outputPath = config["FinancialModelingPrep:OutputFilePath"];

                        return new FinancialModellingPrepService(httpClient, repository, historicalDataRepository, instrumentRepository, apiKey, baseUrl, outputPath);
                    });

                    // Register the default IMarketDataService (use Yahoo Finance by default, or configure via settings)
                    services.AddSingleton<IMarketDataService>(provider =>
                    {
                        var config = provider.GetRequiredService<IConfiguration>();
                        var preferredService = config["MarketData:PreferredService"];

                        return preferredService?.ToLower() switch
                        {
                            "fmp" => provider.GetRequiredService<FinancialModellingPrepService>(),
                            _ => provider.GetRequiredService<FinancialModellingPrepService>() // Default to FMP for backwards compatibility
                        };
                    });
                    services.AddSingleton<IReportFetchingService>(provider =>
                    {
                        // Resolve the factory itself, not an instance
                        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                        var config = provider.GetRequiredService<IConfiguration>();

                        // Pass the factory as the second argument
                        return new IKBRReportFetchingService(config, httpClientFactory);
                    });
                    services.AddSingleton<IReportRunnerService, ReportRunnerService>();
                    services.AddSingleton<IExcelReportService, ExcelReportService>();
                    services.AddScoped<ITradeExecutionService, TradeExecutionService>(provider =>
                    {
                        var tradeExecutionRepo = provider.GetRequiredService<ITradeExecutionRepository>();
                        var positionRepo = provider.GetRequiredService<IPositionRepository>();
                        return new TradeExecutionService(tradeExecutionRepo, positionRepo);
                    });
                    services.AddSingleton<ITradeHistoryReportService, TradeHistoryService>();
                    services.AddSingleton<IChartDataService, ChartDataService>();
                    services.AddSingleton<ICanSlimScreenerService, CanSlimScreenerService>();
                    services.AddSingleton<IRiskMatrixService, RiskMatrixService>();
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
