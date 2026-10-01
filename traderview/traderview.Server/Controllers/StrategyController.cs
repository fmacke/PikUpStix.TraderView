using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using traderview.Server.DTOs;
using TraderView.Application.Interfaces.Services;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/strategy")]
    public class StrategyController : ControllerBase
    {
        private readonly ILogger<StrategyController> _logger;
        private readonly IStrategyService _strategyService;
        private readonly IMapper _mapper;

        public StrategyController(ILogger<StrategyController> logger, IStrategyService strategyService, IMapper mapper)
        {
            _logger = logger;
            _strategyService = strategyService;
            _mapper = mapper;
        }
        [HttpGet]
        [ProducesResponseType(typeof(List<StrategyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<StrategyDto>>> GetAllStrategiesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all strategies");
                var strategies = await _strategyService.GetAllAsync();
                _logger.LogInformation("Found {Count} strategies", strategies.Count);
                var dto = _mapper.Map<IReadOnlyList<StrategyDto>>(strategies);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching strategies");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching strategies", detail = ex.Message }
                );
            }
        }
    }
}
