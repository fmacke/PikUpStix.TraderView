using TradeViewer.API.Services;
using TradeViewer.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using IKBR_Report_Puller.Interfaces;
using IKBR_Report_Puller.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register IKBR services
builder.Services.AddScoped<IDataService, DataService>();
builder.Services.AddScoped<ITradeHistoryReportService, TradeHistoryService>();

// Register TradeViewer service
builder.Services.AddScoped<ITradeViewerService, TradeViewerService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowReactApp");

// API Endpoints

// GET /api/trades - Get all trades
app.MapGet("/api/trades", async (ITradeViewerService tradeService) =>
{
    try
    {
        var trades = await tradeService.GetAllTradesAsync();
        return Results.Ok(trades);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Error fetching trades"
        );
    }
})
.WithName("GetTrades")
.WithOpenApi();

// GET /api/trades/{id} - Get trade details
app.MapGet("/api/trades/{id:long}", async (long id, ITradeViewerService tradeService) =>
{
    try
    {
        var tradeDetail = await tradeService.GetTradeDetailAsync(id);

        if (tradeDetail == null)
        {
            return Results.NotFound(new { message = $"Trade with ID {id} not found" });
        }

        return Results.Ok(tradeDetail);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Error fetching trade detail"
        );
    }
})
.WithName("GetTradeDetail")
.WithOpenApi();

// GET /api/trades/{id}/context - Get trade with candlestick data
app.MapGet("/api/trades/{id:long}/context", async (
    long id, 
    ITradeViewerService tradeService,
    [FromQuery] int daysBefore = 30, 
    [FromQuery] int daysAfter = 30) =>
{
    try
    {
        var tradeContext = await tradeService.GetTradeContextAsync(id, daysBefore, daysAfter);

        if (tradeContext == null)
        {
            return Results.NotFound(new { message = $"Trade with ID {id} not found" });
        }

        return Results.Ok(tradeContext);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Error fetching trade context"
        );
    }
})
.WithName("GetTradeContext")
.WithOpenApi();

app.Run();

