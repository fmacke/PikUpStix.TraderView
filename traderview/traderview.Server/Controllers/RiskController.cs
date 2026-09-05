using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using traderview.Server.DTOs;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities.FMP;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/risk")]
    public class RiskController : ControllerBase
    {
        private readonly ILogger<RiskController> _logger;
        private readonly IRiskMatrixService _riskMatrixService;
        private readonly IMapper _mapper;
        public RiskController(ILogger<RiskController> logger, IRiskMatrixService riskMatrixService, IMapper mapper) {
            _logger = logger;
            _riskMatrixService = riskMatrixService;
            _mapper = mapper;
        }
        [HttpGet("positionreview")]
        [ProducesResponseType(typeof(RiskMatrixCalculationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RiskMatrixCalculationResultDto>> PositionReview()
        {
            try
            {
                var riskMatrixCalculationRequest = new RiskMatrixCalculationRequest()
                {
                    WinRatePercentage = 30,
                    GainPercentage = 4,
                    LossPercentage = 2,
                    NumberOfTrades = 10
                };
                var trades = await _riskMatrixService.CalculateExpectedRoi(riskMatrixCalculationRequest);
                var tradesDto = _mapper.Map<RiskMatrixCalculationResultDto>(trades);
                return Ok(tradesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trades");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching trades", detail = ex.Message }
                );
            }
        }
    }
}
