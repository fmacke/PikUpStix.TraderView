using AutoMapper;
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
        private readonly IOpenPositionReportService _openPositionReportService;
        private readonly IPositionService _positionService;
        private readonly ITradeHistoryReportService _tradeHistoryReportService;
        private readonly ICompanyReportService _companyReportService;
        private readonly IMapper _mapper;

        public ReportsController(ILogger<ReportsController> logger, IOpenPositionReportService openPositionsReportService, IPositionService positionService, ITradeHistoryReportService tradeHistoryReportService, ICompanyReportService companyReportService, IMapper mapper)
        {
            _logger = logger;
            _openPositionReportService = openPositionsReportService;
            _tradeHistoryReportService = tradeHistoryReportService;
            _positionService = positionService;
            _companyReportService = companyReportService;
            _mapper = mapper;
        }

        [HttpGet("position-outcomes")]
        [ProducesResponseType(typeof(List<PositionOutcomeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<PositionOutcomeDto>>> GetPositionOutcomes()
        {
            try
            {
                _logger.LogInformation("GetPositionOutcomes called");
                var positions = await _positionService.GetClosedPositionsAsync();
                var outcomes = await _tradeHistoryReportService.CreateTradeHistoryReport(positions);
                _logger.LogInformation("CalculatePositionOutcomesAsync returned {Count} items", outcomes?.Count ?? 0);

                var dtos = outcomes.Select(o => new PositionOutcomeDto
                {
                    Id = o.Id,
                    OpenDate = o.OpenDate,
                    CloseDate = o.CloseDate,
                    Status = null,
                    InstrumentId = o.InstrumentId,
                    SymbolName = o.Symbol,
                    LastReportedPrice = null,
                    LastReportedPriceUpdated = null,
                    MaximumFavourableExcursion = o.MaximumFavourableExcursion,
                    MaximumAdverseExcursion = o.MaximumAdverseExcursion,
                    Margin = o.NetMargin,
                    MarketRegime = o.MarketRegime
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
                var outcome = await _openPositionReportService.GetPositionReport(position);
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

        [HttpGet("company-checklist/{symbol}")]
        [ProducesResponseType(typeof(CompanyChecklistReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CompanyChecklistReportDto>> GetCompanyChecklistReport(string symbol)
        {
            try
            {
                _logger.LogInformation("GetCompanyChecklistReport called for symbol={Symbol}", symbol);
                var report = await _companyReportService.GenerateChecklistReportAsync(symbol);

                if (report == null)
                {
                    return NotFound(new { message = $"No checklist report found for symbol {symbol}" });
                }

                var dto = _mapper.Map<CompanyChecklistReportDto>(report);
                _logger.LogInformation("Successfully generated company checklist report for {Symbol}", symbol);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating company checklist report for symbol={Symbol}", symbol);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error generating company checklist report", detail = ex.Message });
            }
        }
    }
}
