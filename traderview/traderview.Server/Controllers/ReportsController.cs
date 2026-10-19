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

        public ReportsController(ILogger<ReportsController> logger, ITradingReportService tradingReportService)
        {
            _logger = logger;
            _tradingReportService = tradingReportService;
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
                    LastReportedPrice = o.LastReportedPrice,
                    LastReportedPriceUpdated = o.LastReportedPriceUpdated,
                    MarketRegime = o.MarketRegime,
                    MaximumFavourableExcursion = o.MaximumFavourableExcursion,
                    MaximumAdverseExcursion = o.MaximumAdverseExcursion
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching position outcomes report");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error generating report", detail = ex.Message });
            }
        }
    }
}
