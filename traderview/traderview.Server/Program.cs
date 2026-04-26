using traderview.Server.Services;
using IKBR_Report_Puller.Interfaces;
using IKBR_Report_Puller.Services;
using IKBR_Report_Puller.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

// Register IKBR services
builder.Services.AddScoped<IDataService, DataService>();
builder.Services.AddScoped<ITradeHistoryReportService, TradeHistoryService>();

// Register TradeViewer service
builder.Services.AddScoped<ITradeViewerService, TradeViewerService>();

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
