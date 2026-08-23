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
        [HttpGet("EvaluateCurrentQuarterEpsAsync/{symbol}")]
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

        /// <summary>
        /// Run the stock screener to get a list of qualifying CAN SLIM candidates
        /// </summary>
        /// <param name="symbol">The stock symbol to screen for CAN SLIM candidates</param>
        /// <returns>A list of qualifying CAN SLIM candidates</returns>
        [HttpGet("RunStockScreener/")]
        [ProducesResponseType(typeof(IReadOnlyList<CanSlimCandidate>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyList<CanSlimCandidate>>> RunStockScreener(string symbol)
        {
            try
            {
                _logger.LogInformation("Fetching all qualifying CAN SLIM candidates");
                var stocksShortList = await _marketDataService.RunScreenerAsync(new CanSlimScreenerCriteria());
                return Ok(stocksShortList);
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

