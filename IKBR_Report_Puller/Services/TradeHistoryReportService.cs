using TraderView.Application.Interfaces.Services;
using TraderView.Application.Models;
using TraderView.Application.Models.ResultBasedForecasting;
using TraderView.Domain.Entities;

namespace PikUpStix.TraderView.Services
{
    public class TradeHistoryReportService : ITradeHistoryReportService
    {
        async Task<TradeHistoryReport> ITradeHistoryReportService.CreateTradeHistoryReport(IReadOnlyList<Position> positions)
        {
            return await Task.FromResult(new TradeHistoryReport(positions));
        }
        async Task<DesiredPerformanceResults> ITradeHistoryReportService.CreateDesiredOutcomeForecast(decimal portfolioSize, decimal positionSizePercent, decimal desiredReturnPercent, IReadOnlyList<Position> positions)
        {
            var trades = await Task.FromResult(new TradeHistoryReport(positions));
            var desiredPerformaceForecast = new DesiredPerformanceResults(
                portfolioSize, 
                positionSizePercent, 
                desiredReturnPercent, 
                trades.GainPercentage / 100,             // Convert from percentage to decimal
                Math.Abs(trades.LossPercentage) / 100,  // Convert from percentage to decimal (LossPercentage is negative)
                trades.WinRatePercentage / 100            // Convert from percentage to decimal
            );
            return await Task.FromResult(desiredPerformaceForecast);
        }
    }
}


