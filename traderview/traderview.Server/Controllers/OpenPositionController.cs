using Microsoft.AspNetCore.Mvc;
using traderview.Server.Services;
using traderview.Server.DTOs;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/openpositions")]
    public class OpenPositionController : ControllerBase
    {
        private readonly IOpenPositionService _openPositionService;
        private readonly ILogger<OpenPositionController> _logger;

        public OpenPositionController(
            IOpenPositionService openPositionService,
            ILogger<OpenPositionController> logger)
        {
            _openPositionService = openPositionService;
            _logger = logger;
        }

        /// <summary>
        /// Get all open positions
        /// </summary>
        /// <returns>List of all open positions</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<OpenPositionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<OpenPositionDto>>> GetAllOpenPositionsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all open positions");
                var openPositions = await _openPositionService.GetAllOpenPositionsAsync();
                _logger.LogInformation("Found {Count} open positions", openPositions.Count);
                return Ok(openPositions);
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
    }
}
