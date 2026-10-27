using Microsoft.AspNetCore.Mvc;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Interfaces.Services;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarketDataController : ControllerBase
    {
        private readonly IMarketDataService _marketDataService;
        private readonly IHistoricalDataService _historicalDataService;
        private readonly ILogger<MarketDataController> _logger;

        public MarketDataController(ILogger<MarketDataController> logger, IMarketDataService marketDataService, IHistoricalDataService historicalDataService)
        {
            _logger = logger;
            _marketDataService = marketDataService;
            _historicalDataService = historicalDataService;
        }

        [HttpGet("exchange-rate")]
        public async Task<ActionResult<decimal>> GetExchangeRate([FromQuery] string baseCurrency, [FromQuery] string quote)
        {
            if (string.IsNullOrWhiteSpace(baseCurrency) || string.IsNullOrWhiteSpace(quote))
            {
                return BadRequest("Both baseCurrency and quote query parameters are required.");
            }

            try
            {
                if (baseCurrency.Trim().Equals(quote.Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    return Ok(1m);
                }

                var rate = await _marketDataService.GetExchangeRateAsync(baseCurrency.Trim(), quote.Trim());
                return Ok(rate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exchange rate for {Base}->{Quote}", baseCurrency, quote);
                return StatusCode(500, "Error fetching exchange rate");
            }
        }

        [HttpGet("regime")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> GetMarketRegime(CancellationToken cancellationToken)
        {
            try
            {
                // Use server-side current time as the open date
                var regime = await _historicalDataService.DetermineRegimeAsync(DateTime.Now, cancellationToken);
                return Ok(regime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining market regime");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error determining market regime", detail = ex.Message });
            }
        }
    }
}
