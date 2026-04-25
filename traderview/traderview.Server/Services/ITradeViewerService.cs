using traderview.Server.DTOs;

namespace traderview.Server.Services
{
    public interface ITradeViewerService
    {
        Task<List<TradeDto>> GetAllTradesAsync();
        Task<TradeDetailDto?> GetTradeDetailAsync(long tradeId);
        Task<TradeContextDto?> GetTradeContextAsync(long tradeId, int daysBefore = 150, int daysAfter = 150);
    }
}
