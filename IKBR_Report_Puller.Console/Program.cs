using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PikUpStix.TraderView.Services;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Infrastructure.Data;
using PikUpStix.TraderView.Services.MarketData;
using TraderView.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
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

                    // Register DB connection factory from configuration
                    services.AddSingleton<IDbConnectionFactory>(provider =>
                    {
                        var config = provider.GetRequiredService<IConfiguration>();
                        var connectionString = BuildConnectionString(config);
                        return new TraderView.Infrastructure.Data.SqlConnectionFactory(connectionString);
                    });

                    // Register EF Core DbContext so InstrumentRepository can use AppDbContext when available
                    services.AddDbContext<TraderView.Infrastructure.DbContexts.AppDbContext>(options =>
                    {
                        options.UseSqlServer(BuildConnectionString(hostContext.Configuration));
                    });

                    // Register repositories 
                    // Note: InstrumentRepository must be registered before TradeExecutionRepository due to dependency
                    services.AddScoped<IInstrumentRepository>(provider =>
                    {
                        var db = provider.GetRequiredService<TraderView.Infrastructure.DbContexts.AppDbContext>();
                        var factory = provider.GetRequiredService<IDbConnectionFactory>();
                        return new InstrumentRepository(db, factory);
                    });

                    services.AddScoped<IPositionRepository>(provider =>
                    {
                        var factory = provider.GetRequiredService<IDbConnectionFactory>();
                        var instrumentRepo = provider.GetRequiredService<IInstrumentRepository>();
                        return new PositionRepository(factory, instrumentRepo);
                    });

                    services.AddScoped<ITradeExecutionRepository>(provider =>
                    {
                        var factory = provider.GetRequiredService<IDbConnectionFactory>();
                        var instrumentRepo = provider.GetRequiredService<IInstrumentRepository>();
                        return new TradeExecutionRepository(factory, instrumentRepo);
                    });

                    services.AddSingleton<IHistoricalDataRepository>(provider =>
                    {
                        var factory = provider.GetRequiredService<IDbConnectionFactory>();
                        return new HistoricalDataRepository(factory);
                    });

                    services.AddSingleton<IEconomicCalendarRepository>(provider =>
                    {
                        var factory = provider.GetRequiredService<IDbConnectionFactory>();
                        return new EconomicCalendarRepository(factory);
                    });

                    services.AddSingleton<ICanSlimCandidateRepository>(provider =>
                    {
                        var factory = provider.GetRequiredService<IDbConnectionFactory>();
                        return new CanSlimCandidateRepository(factory);
                    });

                    services.AddSingleton<IEquitySummaryRepository>(provider =>
                    {
                        var factory = provider.GetRequiredService<IDbConnectionFactory>();
                        return new EquitySummaryRepository(factory);
                    });

                    // Register both market data services
                    services.AddSingleton<FinancialModellingPrepService>(provider =>
                    {
                        var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("IKBR");
                        var repository = provider.GetRequiredService<IEconomicCalendarRepository>();
                        var historicalDataRepository = provider.GetRequiredService<IHistoricalDataRepository>();
                        var instrumentRepository = provider.GetRequiredService<IInstrumentRepository>();
                        var config = provider.GetRequiredService<IConfiguration>();
                        var canSlimCandidateService = provider.GetRequiredService<ICanSlimScreenerService>();
                        var apiKey = config["FinancialModelingPrep:ApiKey"];
                        var baseUrl = config["FinancialModelingPrep:BaseUrl"];
                        var outputPath = config["FinancialModelingPrep:OutputFilePath"];

                        return new FinancialModellingPrepService(httpClient, repository, historicalDataRepository, instrumentRepository, canSlimCandidateService, apiKey, baseUrl, outputPath);
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
                    services.AddScoped<IReportRunnerService, ReportRunnerService>();
                    services.AddSingleton<IExcelReportService, ExcelReportService>();
                    services.AddScoped<IOpenPositionsService, OpenPositionsService>(provider =>
                    {
                        var tradeExecutionRepo = provider.GetRequiredService<ITradeExecutionRepository>();
                        var positionRepo = provider.GetRequiredService<IPositionRepository>();
                        return new OpenPositionsService(tradeExecutionRepo, positionRepo);
                    });
                    services.AddSingleton<ITradeHistoryReportService, TradeHistoryService>();
                    services.AddSingleton<IChartDataService, ChartDataService>();
                    services.AddSingleton<ICanSlimScreenerService, CanSlimScreenerService>();
                    services.AddSingleton<ICurrentPerformanceService, CurrentPerformanceService>();
                    services.AddSingleton<IEquitySummaryService, EquitySummaryService>();
                    services.AddScoped<Application>();
                })
                .Build();

            using var scope = host.Services.CreateScope();
            var app = scope.ServiceProvider.GetRequiredService<Application>();
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
