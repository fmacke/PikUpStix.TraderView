using PikUpStix.TraderView.Services;
using TraderView.Application.Interfaces.Persistence;
using PikUpStix.TraderView.Services.MarketData;
using traderview.Server.DTOs.Mappers;
using traderview.Server.Services;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Services;
using TraderView.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using TraderView.Infrastructure.DbContexts;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register HttpClient and HttpClientFactory
        builder.Services.AddHttpClient();

        // Build connection string and register DB connection factory and AppDbContext
        var connectionString = BuildConnectionString(builder.Configuration);

        builder.Services.AddSingleton<IDbConnectionFactory>(provider =>
        {
            return new TraderView.Infrastructure.Data.SqlConnectionFactory(connectionString);
        });

        // Register EF Core DbContext for repositories that use AppDbContext
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        // Register repositories 
        // Note: InstrumentRepository must be registered before TradeExecutionRepository due to dependency
        builder.Services.AddScoped<IInstrumentRepository>(provider =>
        {
            AppDbContext db = provider.GetRequiredService<AppDbContext>();
            var factory = provider.GetRequiredService<IDbConnectionFactory>();
            return new InstrumentRepository(db, factory);
        });

        builder.Services.AddScoped<IPositionRepository>(provider =>
        {
            AppDbContext db = provider.GetRequiredService<AppDbContext>();
            return new PositionRepository(db);
        });

        builder.Services.AddScoped<ITradeExecutionRepository>(provider =>
        {
            var factory = provider.GetRequiredService<IDbConnectionFactory>();
            var instrumentRepo = provider.GetRequiredService<IInstrumentRepository>();
            return new TradeExecutionRepository(factory, instrumentRepo);
        });

        builder.Services.AddScoped<IHistoricalDataRepository>(provider =>
        {
            var factory = provider.GetRequiredService<IDbConnectionFactory>();
            return new HistoricalDataRepository(factory);
        });

        builder.Services.AddScoped<IEconomicCalendarRepository>(provider =>
        {
            var factory = provider.GetRequiredService<IDbConnectionFactory>();
            return new EconomicCalendarRepository(factory);
        });
        builder.Services.AddSingleton<ICanSlimCandidateRepository>(provider =>
        {
            var factory = provider.GetRequiredService<IDbConnectionFactory>();
            return new CanSlimCandidateRepository(factory);
        });
        builder.Services.AddScoped<INoteRepository>(provider =>
        {
            var factory = provider.GetRequiredService<IDbConnectionFactory>();
            return new NoteRepository(factory);
        });
        builder.Services.AddScoped<IListRepository>(provider =>
        {
            AppDbContext db = provider.GetRequiredService<AppDbContext>();
            return new ListRepository(db);
        });

        builder.Services.AddScoped<IEquitySummaryRepository>(provider =>
        {
            AppDbContext db = provider.GetRequiredService<AppDbContext>();
            return new EquitySummaryRepository(db);
        });

        // Register custom services        
        builder.Services.AddScoped<ITradeHistoryReportService, TradeHistoryService>();
        builder.Services.AddScoped<IReportFetchingService, IKBRReportFetchingService>();
        builder.Services.AddScoped<IReportRunnerService, ReportRunnerService>();
        builder.Services.AddScoped<IExcelReportService, ExcelReportService>();
        builder.Services.AddScoped<ITradeExecutionService, TradeExecutionService>();
        builder.Services.AddScoped<IOpenPositionsService, OpenPositionsService>(provider =>
        {
            var tradeExecutionRepo = provider.GetRequiredService<ITradeExecutionRepository>();
            var positionRepo = provider.GetRequiredService<IPositionRepository>();
            return new OpenPositionsService(tradeExecutionRepo, positionRepo);
        });
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
        builder.Services.AddScoped<FinancialModellingPrepCompanyScreeningService>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            var canSlimCandidateService = provider.GetRequiredService<ICanSlimScreenerService>();
            var apiKey = config["FinancialModelingPrep:ApiKey"];
            var baseUrl = config["FinancialModelingPrep:BaseUrl"];
            return new FinancialModellingPrepCompanyScreeningService(httpClient, canSlimCandidateService, apiKey, baseUrl);
        });
        builder.Services.AddScoped<IMarketDataService>(provider =>
        {
            return provider.GetRequiredService<FinancialModellingPrepService>();
        });
        builder.Services.AddScoped<ICompanyScreeningService>(provider =>
        {
            return provider.GetRequiredService<FinancialModellingPrepCompanyScreeningService>();
        });
        builder.Services.AddScoped<IListService, ListService>();
        builder.Services.AddScoped<INoteService, NoteService>();
        builder.Services.AddScoped<ICanSlimScreenerService, CanSlimScreenerService>();
        builder.Services.AddScoped<ICurrentPerformanceService, CurrentPerformanceService>();
        builder.Services.AddScoped<ITradeViewerService, TradeViewerService>();
        builder.Services.AddScoped<IDesiredPerformanceForecastService, DesiredPerformanceForecastService>();
        builder.Services.AddScoped<ITradeCalculatorService, TradeCalculatorService>();
        builder.Services.AddScoped<IEquitySummaryService, EquitySummaryService>();


        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Register mapping for static assets
        builder.Services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<CurrentPerformanceProfile>();
            cfg.AddProfile<DesiredPerformanceResultsProfile>();
        });

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