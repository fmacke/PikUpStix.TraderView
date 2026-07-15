using traderview.Server.DTOs;

namespace traderview.Server.Services
{
    /// <summary>
    /// Service interface for Trade Viewer operations
    /// </summary>
    public interface ITradeViewerService
    {
        /// <summary>
        /// Gets all trades asynchronously
        /// </summary>
        /// <returns>List of all trades</returns>
        Task<List<TradeDto>> GetAllTradesAsync();

        /// <summary>
        /// Gets detailed information for a specific trade asynchronously
        /// </summary>
        /// <param name="positionId">The position ID</param>
        /// <returns>Trade detail information or null if not found</returns>
        Task<TradeDetailDto?> GetTradeDetailAsync(int positionId);

        /// <summary>
        /// Gets trade context including candlestick data asynchronously
        /// </summary>
        /// <param name="positionId">The position ID</param>
        /// <param name="daysBefore">Number of days before trade entry</param>
        /// <param name="daysAfter">Number of days after trade exit</param>
        /// <returns>Trade context data or null if not found</returns>
        Task<TradeContextDto?> GetTradeContextAsync(int positionId, int daysBefore = 150, int daysAfter = 150);

        /// <summary>
        /// Gets Relative Strength indicator data for a trade asynchronously
        /// </summary>
        /// <param name="positionId">The position ID</param>
        /// <param name="benchmarkSymbol">The benchmark symbol for comparison</param>
        /// <param name="daysBefore">Number of days before trade entry</param>
        /// <param name="daysAfter">Number of days after trade exit</param>
        /// <returns>RS indicator data or null if not found</returns>
        Task<RSIndicatorDataDto?> GetRSIndicatorDataAsync(int positionId, string benchmarkSymbol = "^GSPC", int daysBefore = 150, int daysAfter = 150);
    }
}
