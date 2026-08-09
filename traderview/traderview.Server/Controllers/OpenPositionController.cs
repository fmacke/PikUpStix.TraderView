using Microsoft.AspNetCore.Mvc;
using PikUpStix.TraderView.Interfaces;
using traderview.Server.DTOs;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/openpositions")]
    public class OpenPositionController : ControllerBase
    {
        private readonly ILogger<OpenPositionController> _logger;
        private readonly ITradeExecutionService _tradeExecutionService;
        private readonly IExcelReportService _excelReportService;

        public OpenPositionController(
            ILogger<OpenPositionController> logger,
            ITradeExecutionService tradeExecutionService,
            IExcelReportService excelReportService)
        {
            _logger = logger;
            _tradeExecutionService = tradeExecutionService;
            _excelReportService = excelReportService;
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
                var openPositions = await _tradeExecutionService.GetOpenPositionsAsync();
                _logger.LogInformation("Found {Count} open positions", openPositions.Count);

                // Use the shared report data preparation method
                var reportData = _excelReportService.PrepareOpenPositionReportData(openPositions);

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
                    UnrealizedPnL = data.UnrealizedPnL,
                    PercentChange = data.PercentChange,
                    CurrentMargin = data.CurrentMargin,

                    // Keep existing fields for backward compatibility
                    Description = string.Empty,
                    AssetCategory = string.Empty,
                    Currency = string.Empty,
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
    }
}
