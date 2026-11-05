using Microsoft.AspNetCore.Mvc;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/historicalData")]
    public class HistoricalDataController : ControllerBase
    {
        private readonly IMarketDataService _marketDataService;
        private readonly ILogger<HistoricalDataController> _logger;

        public HistoricalDataController(
            IMarketDataService marketDataService,
            ILogger<HistoricalDataController> logger)
        {
            _marketDataService = marketDataService;
            _logger = logger;
        }
        [HttpGet("{conIds}")]
        [ProducesResponseType(typeof(List<HistoricalDatum>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<HistoricalDatum>>> SaveInstrumentData(string conIds)
        {
            try
            {
                _logger.LogInformation("Saving historical data from FMP to database for symbol: {Symbol}", conIds);
                await _marketDataService.FetchAndSaveChartData(conIds.Split(',').ToList(), 700);
                return Ok("success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching historical data for symbol {Symbol}", conIds);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching historical data", detail = ex.Message }
                );
            }
        }
    }
}
