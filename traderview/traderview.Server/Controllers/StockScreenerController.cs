using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Mvc;
using PikUpStix.TraderView.Services;
using traderview.Server.DTOs;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Models.FMP;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/stockscreener")]
    public class StockScreenerController : ControllerBase
    {
        private readonly ILogger<StockScreenerController> _logger;
        private readonly IMarketDataService _marketDataService;

        public StockScreenerController(
            ILogger<StockScreenerController> logger,
            IMarketDataService marketDataService)
        {
            _logger = logger;
            _marketDataService = marketDataService;
        }

        /// <summary>
        /// Get all open positions
        /// </summary>
        /// <returns>List of all open positions</returns>
        [HttpGet("{symbol}")]
        [ProducesResponseType(typeof(CanSlimCurrentQuarterMetric), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CanSlimCurrentQuarterMetric>> EvaluateCurrentQuarterEpsAsync(string symbol)
        {
            try
            {
                _logger.LogInformation("Fetching all qualifying EPS stocks");
                var epsReport = await _marketDataService.EvaluateCurrentQuarterEpsAsync(symbol, 25M, 20M);                
                return Ok(epsReport);
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
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<CanSlimCandidate>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyList<CanSlimCandidate>>> RunStockScreener()
        {
            try
            {
                _logger.LogInformation("Fetching all qualifying EPS stocks");
                var canSlimScreenerCriteria = new CanSlimScreenerCriteria();
                var canslimCandidates = await _marketDataService.RunScreenerAsync(canSlimScreenerCriteria);
                return Ok(canslimCandidates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stocks");
                return StatusCode(  
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching stocks", detail = ex.Message }
                );
            }
        }
    }
}

