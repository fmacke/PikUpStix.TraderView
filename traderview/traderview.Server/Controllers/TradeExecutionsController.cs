using Microsoft.AspNetCore.Mvc;
using traderview.Server.DTOs;
using TraderView.Application.Interfaces.Services;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/tradeexecutions")]
    public class TradeExecutionsController : ControllerBase
    {
        private readonly ITradeExecutionService _tradeExecutionService;
        private readonly ILogger<TradeExecutionsController> _logger;

        public TradeExecutionsController(
            ITradeExecutionService tradeExecutionService,
            ILogger<TradeExecutionsController> logger)
        {
            _tradeExecutionService = tradeExecutionService;
            _logger = logger;
        }

        /// <summary>
        /// Get all trade executions for a specific position
        /// </summary>
        /// <param name="positionId">The position ID to fetch trade executions for</param>
        /// <returns>List of trade executions for the position</returns>
        [HttpGet("position/{positionId}")]
        [ProducesResponseType(typeof(List<TradeExecutionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<TradeExecutionDto>>> GetTradeExecutionsByPositionIdAsync(int positionId)
        {
            try
            {
                _logger.LogInformation("Fetching trade executions for position {PositionId}", positionId);

                var tradeExecutions = await _tradeExecutionService.GetTradeExecutionsByPositionIdAsync(positionId);

                if (tradeExecutions == null || !tradeExecutions.Any())
                {
                    _logger.LogInformation("No trade executions found for position {PositionId}", positionId);
                    return Ok(new List<TradeExecutionDto>());
                }

                // Map TradeExecution entities to TradeExecutionDto
                var tradeExecutionDtos = tradeExecutions.Select(te => new TradeExecutionDto
                {
                    Id = te.Id,
                    PositionId = te.PositionId ?? 0,
                    Symbol = te.Symbol,
                    DateTime = te.DateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Quantity = te.Quantity,
                    TradePrice = te.TradePrice,
                    IbCommission = te.IbCommission,
                    IbCommissionCurrency = te.IbCommissionCurrency,
                    ClosePrice = te.ClosePrice,
                    Cost = te.Cost,
                    FifoPnlRealized = te.FifoPnlRealized,
                    IbExecId = te.IbExecId,
                    BuySell = te.BuySell,
                    TradeId = te.TradeId,
                    TradeDate = te.TradeDate
                }).ToList();

                _logger.LogInformation("Found {Count} trade executions for position {PositionId}", tradeExecutionDtos.Count, positionId);
                return Ok(tradeExecutionDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trade executions for position {PositionId}", positionId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching trade executions", detail = ex.Message }
                );
            }
        }
    }
}
