using TraderView.Application.Models;
using TraderView.Application.Models.ResultBasedForecasting;
using TraderView.Domain.Entities;
using TraderView.Domain.Entities.FMP;

namespace TraderView.Application.Interfaces.Services
{
    public interface ITradeHistoryReportService
    {
        Task<TradeHistoryReport> CreateTradeHistoryReport(IReadOnlyList<Position> positions);
        Task<DesiredPerformanceResults> CreateDesiredOutcomeForecast(decimal portfolioSize, decimal positionSizePercent, decimal desiredReturnPercent, IReadOnlyList<Position> positions);
    }
}
