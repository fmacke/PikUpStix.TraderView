using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PikUpStix.TraderView.Services;
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
        private readonly ITradeHistoryReportService _tradeHistoryService;
        private readonly ITradeExecutionService _tradeExecutionService;

        private readonly IMapper _mapper;
        public RiskController(ILogger<RiskController> logger, IRiskMatrixService riskMatrixService, IMapper mapper, ITradeHistoryReportService tradeHistoryService, ITradeExecutionService tradeExecutionService) {
            _logger = logger;
            _tradeHistoryService = tradeHistoryService;
            _tradeExecutionService = tradeExecutionService;
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
                var executions = await _tradeExecutionService.GetTradeExecutions();
                _tradeHistoryService.CreateTradeHistoryReport(executions);
                var trades = await _riskMatrixService.CalculateExpectedRoi(_tradeHistoryService.RiskMatrixCalculationRequest);
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
