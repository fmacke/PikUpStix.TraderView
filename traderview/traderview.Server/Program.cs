using traderview.Server.Services;
using IKBR_Report_Puller.Interfaces;
using IKBR_Report_Puller.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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
