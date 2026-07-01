using traderview.Server.Services;
using IKBR_Report_Puller.Services;
using IKBR_Report_Puller.Data.Repositories;
using PikUpStix.TraderView.Interfaces;


public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        // Register HttpClient and HttpClientFactory
        builder.Services.AddHttpClient();

        // Register repositories (following Repository Pattern with DI)
        // Note: InstrumentRepository must be registered before TradeExecutionRepository due to dependency
        builder.Services.AddScoped<IInstrumentRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = BuildConnectionString(config);
            return new InstrumentRepository(connectionString);
        });

        builder.Services.AddScoped<ITradeExecutionRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var instrumentRepo = provider.GetRequiredService<IInstrumentRepository>();
            var connectionString = BuildConnectionString(config);
            return new TradeExecutionRepository(connectionString, instrumentRepo);
        });

        builder.Services.AddScoped<IHistoricalDataRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = BuildConnectionString(config);
            return new HistoricalDataRepository(connectionString);
        });

        builder.Services.AddScoped<IOpenPositionRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = BuildConnectionString(config);
            return new OpenPositionRepository(connectionString);
        });

        builder.Services.AddScoped<IEconomicCalendarRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = BuildConnectionString(config);
            return new EconomicCalendarRepository(connectionString);
        });

        // Register IKBR services
        builder.Services.AddScoped<ITradeHistoryReportService, TradeHistoryService>();

        builder.Services.AddScoped<IReportFetchingService, IKBRReportFetchingService>();

        builder.Services.AddScoped<IReportRunnerService, ReportRunnerService>();

        builder.Services.AddScoped<IExcelReportService, ExcelReportService>();

        builder.Services.AddScoped<IMarketDataService>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            var economicRepo = provider.GetRequiredService<IEconomicCalendarRepository>();
            var historicalRepo = provider.GetRequiredService<IHistoricalDataRepository>();
            var instrumentRepo = provider.GetRequiredService<IInstrumentRepository>();
            var apiKey = config["FinancialModelingPrep:ApiKey"];
            var baseUrl = config["FinancialModelingPrep:BaseUrl"];
            var outputFilePath = config["IBKR:OutputFilePath"];
            return new FinancialModellingPrepService(httpClient, economicRepo, historicalRepo, instrumentRepo, apiKey, baseUrl, outputFilePath);
        });


        // Register TradeViewer service
        builder.Services.AddScoped<ITradeViewerService, TradeViewerService>();

        // Register OpenPosition service
        builder.Services.AddScoped<IOpenPositionService, OpenPositionService>();

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        app.UseDefaultFiles();
        app.MapStaticAssets();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.MapFallbackToFile("/index.html");

        app.Run();

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