using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using PikUpStix.TraderView.Services;
using PikUpStix.TraderView.Services.MarketData;
using traderview.Server.Services;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Infrastructure.Repositories;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register HttpClient and HttpClientFactory
        builder.Services.AddHttpClient();

        // Register repositories 
        // Note: InstrumentRepository must be registered before TradeExecutionRepository due to dependency
        builder.Services.AddScoped<IInstrumentRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = BuildConnectionString(config);
            return new InstrumentRepository(connectionString);
        });

        builder.Services.AddScoped<IPositionRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var instrumentRepo = provider.GetRequiredService<IInstrumentRepository>();
            var connectionString = BuildConnectionString(config);
            return new PositionRepository(connectionString, instrumentRepo);
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

        builder.Services.AddScoped<IEconomicCalendarRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = BuildConnectionString(config);
            return new EconomicCalendarRepository(connectionString);
        });
        builder.Services.AddSingleton<ICanSlimCandidateRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = BuildConnectionString(config);
            return new CanSlimCandidateRepository(connectionString);
        });
        builder.Services.AddScoped<INoteRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = BuildConnectionString(config);
            return new NoteRepository(connectionString);
        });
        builder.Services.AddScoped<IListRepository>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = BuildConnectionString(config);
            return new ListRepository(connectionString);
        });

        // Register custom services        
        builder.Services.AddScoped<ITradeHistoryReportService, TradeHistoryService>();
        builder.Services.AddScoped<IReportFetchingService, IKBRReportFetchingService>();
        builder.Services.AddScoped<IReportRunnerService, ReportRunnerService>();
        builder.Services.AddScoped<IExcelReportService, ExcelReportService>();
        builder.Services.AddScoped<ITradeExecutionService, TradeExecutionService>(provider =>
        {
            var tradeExecutionRepo = provider.GetRequiredService<ITradeExecutionRepository>();
            var positionRepo = provider.GetRequiredService<IPositionRepository>();
            return new TradeExecutionService(tradeExecutionRepo, positionRepo);
        });

        // Register FinancialModellingPrepService
        builder.Services.AddScoped<FinancialModellingPrepService>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            var economicRepo = provider.GetRequiredService<IEconomicCalendarRepository>();
            var historicalRepo = provider.GetRequiredService<IHistoricalDataRepository>();
            var instrumentRepo = provider.GetRequiredService<IInstrumentRepository>();
            var apiKey = config["FinancialModelingPrep:ApiKey"];
            var baseUrl = config["FinancialModelingPrep:BaseUrl"];
            var outputFilePath = config["FinancialModelingPrep:OutputFilePath"];
            return new FinancialModellingPrepService(httpClient, economicRepo, historicalRepo, instrumentRepo, apiKey, baseUrl, outputFilePath);
        });

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
            var outputFilePath = config["FinancialModelingPrep:OutputFilePath"];
            return new FinancialModellingPrepService(httpClient, economicRepo, historicalRepo, instrumentRepo, apiKey, baseUrl, outputFilePath);
        });
        builder.Services.AddScoped<IListService, ListService>();
        builder.Services.AddScoped<INoteService, NoteService>();
        builder.Services.AddScoped<ICanSlimScreenerService, CanSlimScreenerService>();
        builder.Services.AddScoped<ITradeViewerService, TradeViewerService>();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.UseDefaultFiles();
        app.MapStaticAssets();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
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