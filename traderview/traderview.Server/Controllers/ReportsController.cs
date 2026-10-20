using Microsoft.AspNetCore.Mvc;
using traderview.Server.DTOs;
using TraderView.Application.Interfaces.Services;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly ILogger<ReportsController> _logger;
        private readonly ITradingReportService _tradingReportService;
        private readonly IPositionService _positionService;

        public ReportsController(ILogger<ReportsController> logger, ITradingReportService tradingReportService, IPositionService positionService)
        {
            _logger = logger;
            _tradingReportService = tradingReportService;
            _positionService = positionService;
        }

        [HttpGet("position-outcomes")]
        [ProducesResponseType(typeof(List<PositionOutcomeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<PositionOutcomeDto>>> GetPositionOutcomes()
        {
            try
            {
                _logger.LogInformation("GetPositionOutcomes called");
                var outcomes = await _tradingReportService.CalculatePositionOutcomesAsync();
                _logger.LogInformation("CalculatePositionOutcomesAsync returned {Count} items", outcomes?.Count ?? 0);

                var dtos = outcomes.Select(o => new PositionOutcomeDto
                {
                    Id = o.Id,
                    OpenDate = o.OpenDate,
                    CloseDate = o.CloseDate,
                    Status = o.Status,
                    InstrumentId = o.InstrumentId,
                    SymbolName = o.SymbolName,
                    LastReportedPrice = o.LastReportedPrice,
                    LastReportedPriceUpdated = o.LastReportedPriceUpdated,
                    MarketRegime = o.MarketRegime,
                    MaximumFavourableExcursion = o.MaximumFavourableExcursion,
                    MaximumAdverseExcursion = o.MaximumAdverseExcursion,
                    Margin = o.Margin
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching position outcomes report");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error generating report", detail = ex.Message });
            }
        }
        [HttpGet("position-outcome")]
        [ProducesResponseType(typeof(List<PositionOutcomeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<PositionOutcomeDto>>> GetPositionOutcome(int id)
        {
            try
            {
                _logger.LogInformation("GetPositionOutcome called with id={Id}", id);
                var position = await _positionService.GetPositionByIdAsync(id);
                if (position == null)
                {
                    return NotFound();
                }
                var outcome = await _tradingReportService.GetPositionReport(position);
                if (outcome == null)
                {
                    return NotFound();
                }

                var dto = new PositionOutcomeDto
                {
                    Id = outcome.Id,
                    OpenDate = outcome.OpenDate,
                    CloseDate = outcome.CloseDate,
                    Status = outcome.Status,
                    InstrumentId = outcome.InstrumentId,
                    SymbolName = outcome.SymbolName,
                    LastReportedPrice = outcome.LastReportedPrice,
                    LastReportedPriceUpdated = outcome.LastReportedPriceUpdated,
                    MarketRegime = outcome.MarketRegime,
                    MaximumFavourableExcursion = outcome.MaximumFavourableExcursion,
                    MaximumAdverseExcursion = outcome.MaximumAdverseExcursion,
                    Margin = outcome.Margin
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching position outcomes report");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error generating report", detail = ex.Message });
            }
        }
    }
}
