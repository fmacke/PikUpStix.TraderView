using Microsoft.AspNetCore.Mvc;
using traderview.Server.Services;
using traderview.Server.DTOs;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TradeViewerController : ControllerBase
    {
        private readonly ITradeViewerService _tradeViewerService;
        private readonly ILogger<TradeViewerController> _logger;

        public TradeViewerController(
            ITradeViewerService tradeViewerService,
            ILogger<TradeViewerController> logger)
        {
            _tradeViewerService = tradeViewerService;
            _logger = logger;
        }

        /// <summary>
        /// Get all trades
        /// </summary>
        /// <returns>List of all trades</returns>
        [HttpGet("trades")]
        [ProducesResponseType(typeof(List<TradeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<TradeDto>>> GetAllTradesAsync()
        {
            try
            {
                var trades = await _tradeViewerService.GetAllTradesAsync();
                return Ok(trades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trades");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching trades", detail = ex.Message }
                );
            }
        }

        /// <summary>
        /// Get candlestick data for a specific trade
        /// </summary>
        /// <param name="tradeId">The trade ID</param>
        /// <param name="daysBefore">Number of days before trade entry to include (default: 5)</param>
        /// <param name="daysAfter">Number of days after trade exit to include (default: 5)</param>
        /// <returns>Trade context with candlestick data</returns>
        [HttpGet("trades/{tradeId}/candlesticks")]
        [ProducesResponseType(typeof(TradeContextDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TradeContextDto>> GetTradeCandlesticksAsync(
            long tradeId,
            [FromQuery] int daysBefore = 5,
            [FromQuery] int daysAfter = 5)
        {
            try
            {
                _logger.LogInformation("Fetching candlesticks for trade {TradeId}", tradeId);

                var tradeContext = await _tradeViewerService.GetTradeContextAsync(tradeId, daysBefore, daysAfter);

                if (tradeContext == null)
                {
                    _logger.LogWarning("Trade {TradeId} not found", tradeId);
                    return NotFound(new { message = $"Trade with ID {tradeId} not found" });
                }

                _logger.LogInformation("Found {Count} candlesticks for trade {TradeId}", 
                    tradeContext.Candlesticks.Count, tradeId);

                return Ok(tradeContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching candlesticks for trade {TradeId}", tradeId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching candlestick data", detail = ex.Message }
                );
            }
        }
    }
}
