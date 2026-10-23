using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PikUpStix.TraderView.Services;
using PikUpStix.TraderView.Services.MarketData;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Services;
using TraderView.Infrastructure.Data;
using TraderView.Infrastructure.DbContexts;
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
                        PooledConnectionLifetime = TimeSpan.FromMinutes(15),
                        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(14)
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

                    // No AutoMapper registration here; repository uses EF Core CurrentValues.SetValues for mapping

                    // Register Repositories 
                    services.AddScoped<IInstrumentRepository>(provider =>
                    {
                        AppDbContext db = provider.GetRequiredService<AppDbContext>();
                        return new InstrumentRepository(db);
                    });
                    services.AddScoped<ITradingReportService, TradingReportService>();
                    services.AddScoped<IPositionRepository>(provider =>
                    {
                        AppDbContext db = provider.GetRequiredService<AppDbContext>();
                        return new PositionRepository(db);
                    });

                    services.AddScoped<IInstrumentService, InstrumentService>(provider =>
                    {
                        var instrumentRepo = provider.GetRequiredService<IInstrumentRepository>();
                        return new InstrumentService(instrumentRepo);
                    });
                    services.AddScoped<IPositionService, PositionService>(provider =>
                    {
                        var positionRepo = provider.GetRequiredService<IPositionRepository>();
                        return new PositionService(positionRepo);
                    });
                    services.AddScoped<ITradeExecutionRepository>(provider =>
                    {
                        var db = provider.GetRequiredService<AppDbContext>();
                        return new TradeExecutionRepository(db);
                    });

                    services.AddScoped<IHistoricalDataRepository>(provider =>
                    {
                        var db = provider.GetRequiredService<AppDbContext>();
                        return new HistoricalDataRepository(db);
                    });

                    services.AddScoped<IEconomicCalendarRepository>(provider =>
                    {
                        var db = provider.GetRequiredService<AppDbContext>();
                        return new EconomicCalendarRepository(db);
                    });

                    services.AddScoped<ICanSlimCandidateRepository>(provider =>
                    {
                        var db = provider.GetRequiredService<AppDbContext>();
                        return new CanSlimCandidateRepository(db);
                    });

                    services.AddScoped<IEquitySummaryRepository>(provider =>
                    {
                        var db = provider.GetRequiredService<AppDbContext>();
                        return new EquitySummaryRepository(db);
                    });

                    // Register Services
                    services.AddScoped<IHistoricalDataService, HistoricalDataService>(provider =>
                    {
                        var historicalDataRepo = provider.GetRequiredService<IHistoricalDataRepository>();
                        return new HistoricalDataService(historicalDataRepo);
                    });
                    services.AddScoped<IPositionService, PositionService>(provider =>
                    {
                        var positionRepo = provider.GetRequiredService<IPositionRepository>();
                        return new PositionService(positionRepo);
                    });
                    services.AddScoped<ITradeExecutionService, TradeExecutionService>(provider =>
                    {
                        var tradeExecutionRepo = provider.GetRequiredService<ITradeExecutionRepository>();
                        var positionService = provider.GetRequiredService<IPositionService>();
                        var instrumentService = provider.GetRequiredService<IInstrumentService>();
                        return new TradeExecutionService(tradeExecutionRepo, positionService, instrumentService);
                    });
                    services.AddScoped<ITradingReportService, TradingReportService>(provider =>
                    {
                        var historicalDataService = provider.GetRequiredService<IHistoricalDataService>();
                        var positionService = provider.GetRequiredService<IPositionService>();
                        var tradeExecutionService = provider.GetRequiredService<ITradeExecutionService>();
                        var logger = provider.GetRequiredService<ILogger<TradingReportService>>();
                        return new TradingReportService(historicalDataService, positionService, tradeExecutionService, logger);
                    });
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
                    services.AddScoped<FinancialModellingPrepCompanyScreeningService>(provider =>
                    {
                        var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("IKBR");
                        var canSlimCandidateService = provider.GetRequiredService<ICanSlimScreenerService>();
                        var config = provider.GetRequiredService<IConfiguration>();
                        var apiKey = config["FinancialModelingPrep:ApiKey"];
                        var baseUrl = config["FinancialModelingPrep:BaseUrl"];
                        return new FinancialModellingPrepCompanyScreeningService(httpClient, canSlimCandidateService, apiKey, baseUrl);
                    });
                    services.AddScoped<IMarketDataService>(provider =>
                    {
                        return provider.GetRequiredService<FinancialModellingPrepService>();
                    });
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
                    services.AddScoped<IExcelReportService, ExcelReportService>();
                    services.AddSingleton<ITradeHistoryReportService, TradeHistoryService>();
                    services.AddSingleton<IChartDataService, ChartDataService>();
                    services.AddScoped<ICanSlimScreenerService, CanSlimScreenerService>();
                    services.AddScoped<ICurrentPerformanceService, CurrentPerformanceService>();
                    services.AddScoped<IEquitySummaryService, EquitySummaryService>();
                    services.AddScoped<IReportRunnerService, ReportRunnerService>();
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
