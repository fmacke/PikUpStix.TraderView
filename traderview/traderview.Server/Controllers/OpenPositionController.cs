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

        public OpenPositionController(
            ILogger<OpenPositionController> logger,
            ITradeExecutionService tradeExecutionService)
        {
            _logger = logger;
            _tradeExecutionService = tradeExecutionService;
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

                return Ok(openPositions.Select(p => new OpenPositionDto
                {
                    Symbol = p.Instrument.InstrumentName,
                    Description  = p.Instrument.InstrumentName,
                    AssetCategory  = p.Instrument.ListingExchange,
                    Currency  = p.Instrument.Currency,
                    Position = p.TradeExecutions.Sum(te => te.Quantity),
                    MarkPrice = p.LastReportedPrice,
                    PositionValue = p.TradeExecutions.Sum(te => te.Quantity) * p.LastReportedPrice,
                    CostBasisPrice = p.TradeExecutions.Sum(te => te.Quantity * te.TradePrice) / (p.TradeExecutions.Sum(te => te.Quantity) == 0 ? 1 : p.TradeExecutions.Sum(te => te.Quantity)),
                    CostBasisMoney = p.TradeExecutions.Sum(te => te.Quantity * te.TradePrice),
                    FifoPnlUnrealized = p.TradeExecutions.Sum(te => te.Quantity * (p.LastReportedPrice - te.TradePrice)),
                    PercentOfNAV = p.TradeExecutions.Sum(te => te.Quantity * p.LastReportedPrice) / (p.TradeExecutions.Sum(te => te.Quantity * p.LastReportedPrice) == 0 ? 1 : p.TradeExecutions.Sum(te => te.Quantity * p.LastReportedPrice)),
                    ReportDate = DateTime.UtcNow,
                    ListingExchange = p.Instrument.ListingExchange
                }).ToList());
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
