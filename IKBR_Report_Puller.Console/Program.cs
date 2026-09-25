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
using TraderView.Infrastructure.DbContexts;

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
                        return new SqlConnectionFactory(connectionString);
                    });

                    // Register EF Core DbContext so InstrumentRepository can use AppDbContext when available
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseSqlServer(BuildConnectionString(hostContext.Configuration));
                    });

                    // Register repositories 
                    // Note: InstrumentRepository must be registered before TradeExecutionRepository due to dependency
                    services.AddScoped<IInstrumentRepository>(provider =>
                    {
                        AppDbContext db = provider.GetRequiredService<AppDbContext>();
                        var factory = provider.GetRequiredService<IDbConnectionFactory>();
                        return new InstrumentRepository(db, factory);
                    });

                    services.AddScoped<IPositionRepository>(provider =>
                    {
                        AppDbContext db = provider.GetRequiredService<AppDbContext>();
                        var factory = provider.GetRequiredService<IDbConnectionFactory>();
                        return new PositionRepository(db);
                    });

                    services.AddScoped<ITradeExecutionRepository>(provider =>
                    {
                        var db = provider.GetRequiredService<AppDbContext>();
                        var instrumentService = provider.GetRequiredService<IInstrumentService>();
                        var positionService = provider.GetRequiredService<IPositionService>();
                        return new TradeExecutionRepository(db, instrumentService, positionService);
                    });

                    services.AddSingleton<IHistoricalDataRepository>(provider =>
                    {
                        var factory = provider.GetRequiredService<IDbConnectionFactory>();
                        return new HistoricalDataRepository(factory);
                    });

                    services.AddSingleton<IEconomicCalendarRepository>(provider =>
                    {
                        var db = provider.GetRequiredService<AppDbContext>();
                        return new EconomicCalendarRepository(db);
                    });

                    services.AddSingleton<ICanSlimCandidateRepository>(provider =>
                    {
                        var db = provider.GetRequiredService<AppDbContext>();
                        return new CanSlimCandidateRepository(db);
                    });

                    services.AddSingleton<IEquitySummaryRepository>(provider =>
                    {
                        var db = provider.GetRequiredService<AppDbContext>();
                        return new EquitySummaryRepository(db);
                    });

                    // Register market data service (FinancialModellingPrepService)
                    services.AddScoped<FinancialModellingPrepService>(provider =>
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

                    // Register company screening service (FinancialModellingPrepCompanyScreeningService)
                    services.AddScoped<FinancialModellingPrepCompanyScreeningService>(provider =>
                    {
                        var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("IKBR");
                        var canSlimCandidateService = provider.GetRequiredService<ICanSlimScreenerService>();
                        var config = provider.GetRequiredService<IConfiguration>();
                        var apiKey = config["FinancialModelingPrep:ApiKey"];
                        var baseUrl = config["FinancialModelingPrep:BaseUrl"];

                        return new FinancialModellingPrepCompanyScreeningService(httpClient, canSlimCandidateService, apiKey, baseUrl);
                    });

                    // Register the default IMarketDataService
                    services.AddScoped<IMarketDataService>(provider =>
                    {
                        return provider.GetRequiredService<FinancialModellingPrepService>();
                    });

                    // Register the default ICompanyScreeningService
                    services.AddScoped<ICompanyScreeningService>(provider =>
                    {
                        return provider.GetRequiredService<FinancialModellingPrepCompanyScreeningService>();
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
                    services.AddScoped<IExcelReportService, ExcelReportService>();
                    services.AddScoped<IPositionService, PositionService>(provider =>
                    {
                        var tradeExecutionRepo = provider.GetRequiredService<ITradeExecutionRepository>();
                        var positionRepo = provider.GetRequiredService<IPositionRepository>();
                        return new PositionService(positionRepo);
                    });
                    services.AddSingleton<ITradeHistoryReportService, TradeHistoryService>();
                    services.AddSingleton<IChartDataService, ChartDataService>();
                    services.AddSingleton<ICanSlimScreenerService, CanSlimScreenerService>();
                    services.AddSingleton<ICurrentPerformanceService, CurrentPerformanceService>();
                    services.AddSingleton<IEquitySummaryService, EquitySummaryService>();
                    services.AddScoped<IInstrumentService, InstrumentService>();
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
