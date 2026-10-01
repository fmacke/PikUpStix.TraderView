using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities.FMP;
using traderview.Server.Dtos;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/stockscreener")]
    public class StockScreenerController : ControllerBase
    {
        private readonly ILogger<StockScreenerController> _logger;
        private readonly ICompanyScreeningService _companyScreeningService;
        private readonly IMapper _mapper;

        public StockScreenerController(
            ILogger<StockScreenerController> logger,
            ICompanyScreeningService companyScreeningService,
            IMapper mapper)
        {
            _logger = logger;
            _companyScreeningService = companyScreeningService;
            _mapper = mapper;
        }
        /// <summary>
        /// Run the stock screener to get a list of qualifying CAN SLIM candidates
        /// </summary>
        /// <param name="symbol">The stock symbol to screen for CAN SLIM candidates</param>
        /// <returns>A list of qualifying CAN SLIM candidates</returns>
        [HttpPost("RunStockScreener")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> RunStockScreener()
        {
            try
            {
                _logger.LogInformation("Fetching all qualifying CAN SLIM candidates");
                var stocksShortList = await _companyScreeningService.RunScreenerAsync(new CanSlimScreenerCriteria() { Stage1UniverseLimit = 1000 });
                _logger.LogInformation("IBKR data sync completed successfully");
                return Ok(new { message = "IBKR data sync completed successfully", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching CAN SLIM candidates");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching CAN SLIM candidates", detail = ex.Message }
                );
            }
        }
        /// <summary>
        /// Get the latest list of qualifying CAN SLIM candidates
        /// </summary>
        /// <returns>A list of qualifying CAN SLIM candidates</returns>
        [HttpGet("GetCanSlimCandidates")]
        [ProducesResponseType(typeof(IReadOnlyList<CanSlimCandidateDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyList<CanSlimCandidateDto>>> GetCanSlimCandidates()
        {
            try
            {
                _logger.LogInformation("Fetching all qualifying CAN SLIM candidates");
                var stocksShortList = await _companyScreeningService.GetLatestScreenerResults();
                var dto = _mapper.Map<IReadOnlyList<CanSlimCandidateDto>>(stocksShortList);

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching CAN SLIM candidates");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching CAN SLIM candidates", detail = ex.Message }
                );
            }
        }
    }
}

