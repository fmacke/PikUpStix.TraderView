using Microsoft.AspNetCore.Mvc;
using traderview.Server.DTOs;
using traderview.Server.Services;
using TraderView.Application.Interfaces.Services;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/tradeviewer")]
    public class TradeViewerController : ControllerBase
    {
        private readonly ITradeViewerService _tradeViewerService;
        private readonly IReportRunnerService _reportRunnerService;
        private readonly ILogger<TradeViewerController> _logger;

        public TradeViewerController(
            ITradeViewerService tradeViewerService,
            IReportRunnerService reportRunnerService,
            ILogger<TradeViewerController> logger)
        {
            _tradeViewerService = tradeViewerService;
            _reportRunnerService = reportRunnerService;
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
                foreach (var trade in trades)
                {
                    _logger.LogInformation("Trade ID: {TradeId}, Symbol: {Symbol}, Entry Date: {EntryDate}, Exit Date: {ExitDate}",
                        trade.PositionId, trade.Symbol, trade.EntryDate, trade.ExitDate);
                }
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
        /// <param name="positionId">The trade ID</param>
        /// <param name="daysBefore">Number of calendar days before trade entry to include (default: 150, which typically provides ~100 trading days)</param>
        /// <param name="daysAfter">Number of calendar days after trade exit to include (default: 150, which typically provides ~100 trading days)</param>
        /// <returns>TradeExecution context with candlestick data</returns>
        [HttpGet("trades/{positionId}/candlesticks")]
        [ProducesResponseType(typeof(TradeContextDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TradeContextDto>> GetTradeCandlesticksAsync(
            int positionId,
            [FromQuery] int daysBefore = 150,
            [FromQuery] int daysAfter = 150)
        {
            try
            {
                _logger.LogInformation("Fetching candlesticks for trade {positionId}", positionId);

                var tradeContext = await _tradeViewerService.GetTradeContextAsync(positionId, daysBefore, daysAfter);

                if (tradeContext == null)
                {
                    _logger.LogWarning("Position {positionId} not found", positionId);
                    return NotFound(new { message = $"Position with ID {positionId} not found" });
                }

                _logger.LogInformation("Found {Count} candlesticks for position {positionId}", 
                    tradeContext.Candlesticks.Count, positionId);

                return Ok(tradeContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching candlesticks for position {positionId}", positionId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching candlestick data", detail = ex.Message }
                );
            }
        }

        /// <summary>
        /// Sync IBKR data - fetches reports from Interactive Brokers and updates database
        /// </summary>
        /// <returns>Success message if sync completes successfully</returns>
        [HttpPost("sync")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> SyncIBKRDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting IBKR data sync...");

                await _reportRunnerService.RunReportAsync(false, true);

                _logger.LogInformation("IBKR data sync completed successfully");
                return Ok(new { message = "IBKR data sync completed successfully", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during IBKR data sync");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error during IBKR data sync", detail = ex.Message }
                );
            }
        }
    }
}
