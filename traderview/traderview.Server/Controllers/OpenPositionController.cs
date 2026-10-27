using Microsoft.AspNetCore.Mvc;
using traderview.Server.DTOs;
using TraderView.Application.Interfaces.Services;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/openpositions")]
    public class OpenPositionController : ControllerBase
    {
        private readonly ILogger<OpenPositionController> _logger;
        private readonly IPositionService _openPositionService;
        private readonly ITradingReportService _tradingReportService;
        private readonly IReportRunnerService _reportRunnerService;

        public OpenPositionController(
            ILogger<OpenPositionController> logger,
            IPositionService openPositionsService,
            ITradingReportService tradingReportService,
            IReportRunnerService reportRunnerService,
            IMarketDataService marketDataService)
        {
            _logger = logger;
            _openPositionService = openPositionsService;
            _tradingReportService = tradingReportService;
            _reportRunnerService = reportRunnerService;
        }

        /// <summary>
        /// Get all open positions
        /// </summary>
        /// <returns>List of all open positions</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<OpenPositionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<OpenPositionDto>>> GetAllOpenPositionsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all open positions");
                var openPositions = await _openPositionService.GetOpenPositionsAsync();
                _logger.LogInformation("Found {Count} open positions", openPositions.Count);

                var reportData = await _tradingReportService.PrepareOpenPositionReportData(openPositions);

                // Convert to DTOs
                var openPositionDtos = reportData.Select(data => new OpenPositionDto
                {
                    PositionId = data.PositionId,
                    AccountId = data.AccountId,
                    Symbol = data.Symbol,
                    DateOpened = data.DateOpened,
                    DaysOpened = data.DaysOpened,
                    Quantity = data.Quantity,
                    CostPrice = data.CostPrice,
                    AveragePrice = data.AveragePrice,
                    Value = data.Value,
                    ValueInGBP = data.ValueInGBP,
                    UnrealizedPnL = data.UnrealizedPnL,
                    PercentChange = data.PercentChange,
                    CurrentMargin = data.CurrentMargin,

                    // Keep existing fields for backward compatibility
                    Description = string.Empty,
                    AssetCategory = string.Empty,
                    Currency = data.Currency,
                    Position = data.Quantity,
                    MarkPrice = data.AveragePrice,
                    PositionValue = data.Value,
                    CostBasisPrice = data.CostPrice,
                    CostBasisMoney = data.Quantity * data.CostPrice,
                    FifoPnlUnrealized = data.UnrealizedPnL,
                    PercentOfNAV = null,
                    ReportDate = DateTime.UtcNow,
                    ListingExchange = string.Empty
                }).ToList();

                return Ok(openPositionDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching open positions");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching open positions", detail = ex.Message }
                );
            }
        }

        /// <summary>
        /// Run trade confirm report - fetches trade confirmations from Interactive Brokers and updates database
        /// </summary>
        /// <returns>Success message if report runs successfully</returns>
        [HttpPost("run-trade-confirm-report")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> RunTradeConfirmReportAsync()
        {
            try
            {
                _logger.LogInformation("Starting trade confirm report run...");
                await _reportRunnerService.RunTradeConfirmReport();
                _logger.LogInformation("Trade confirm report run completed successfully");
                return Ok(new { message = "Trade confirm report completed successfully", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running trade confirm report");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error running trade confirm report", detail = ex.Message }
                );
            }
        }
    }
}
