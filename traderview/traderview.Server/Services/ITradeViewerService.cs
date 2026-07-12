using traderview.Server.DTOs;

namespace traderview.Server.Services
{
    public interface ITradeViewerService
    {
        Task<List<TradeDto>> GetAllTradesAsync();
        Task<TradeDetailDto?> GetTradeDetailAsync(int positionId);
        Task<TradeContextDto?> GetTradeContextAsync(int positionId, int daysBefore = 150, int daysAfter = 150);
        Task<RSIndicatorDataDto?> GetRSIndicatorDataAsync(int positionId, string benchmarkSymbol = "^GSPC", int daysBefore = 150, int daysAfter = 150);
    }
}
