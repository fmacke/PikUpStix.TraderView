using Microsoft.AspNetCore.Mvc;
using TraderView.Application.Interfaces.Services;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarketDataController : ControllerBase
    {
        private readonly IMarketDataService _marketDataService;
        private readonly ILogger<MarketDataController> _logger;

        public MarketDataController(ILogger<MarketDataController> logger, IMarketDataService marketDataService)
        {
            _logger = logger;
            _marketDataService = marketDataService;
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

                var rate = await _marketDataService.GetExchangeRate(baseCurrency.Trim(), quote.Trim());
                return Ok(rate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exchange rate for {Base}->{Quote}", baseCurrency, quote);
                return StatusCode(500, "Error fetching exchange rate");
            }
        }
    }
}
