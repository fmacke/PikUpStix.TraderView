using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using traderview.Server.DTOs;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Services;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/risk")]
    public class RiskController : ControllerBase
    {
        private readonly ILogger<RiskController> _logger;
        private readonly ICurrentPerformanceService _riskMatrixService;
        private readonly ITradeHistoryReportService _tradeHistoryService;
        private readonly ITradeExecutionService _tradeExecutionService;
        private readonly IPositionService _positionService;
        private readonly IDesiredPerformanceForecastService _resultsBasedAssumptionForecastService;

        private readonly IMapper _mapper;
        public RiskController(ILogger<RiskController> logger, ICurrentPerformanceService riskMatrixService, IMapper mapper, ITradeHistoryReportService tradeHistoryService, ITradeExecutionService tradeExecutionService, IPositionService positionService, IDesiredPerformanceForecastService resultsBasedAssumptionForecastService) {
            _logger = logger;
            _tradeHistoryService = tradeHistoryService;
            _tradeExecutionService = tradeExecutionService;
            _riskMatrixService = riskMatrixService;
            _positionService = positionService;
            _resultsBasedAssumptionForecastService = resultsBasedAssumptionForecastService;

            _mapper = mapper;
        }
        [HttpGet("currentperformance")]
        [ProducesResponseType(typeof(CurrentPerformanceResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CurrentPerformanceResultDto>> CurrentPerformance()
        {       
            try
            {
                var executions = await _tradeExecutionService.GetTradeExecutionsAsync();
                _tradeHistoryService.CreateTradeHistoryReport(executions);
                var positions = await _positionService.GetClosedPositionsAsync();
                var trades = await _riskMatrixService.CalculateExpectedRoi(_tradeHistoryService.RiskMatrixCalculationRequest, positions);
                var tradesDto = _mapper.Map<CurrentPerformanceResultDto>(trades);
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
        [HttpGet("desiredperformance")]
        [ProducesResponseType(typeof(CurrentPerformanceResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CurrentPerformanceResultDto>> DesiredPerformance(decimal portfolioSize, decimal positionSizePercent, decimal desiredReturnPercent)
        {
            try
            {
                var executions = await _tradeExecutionService.GetTradeExecutionsAsync();
                var inputs = _resultsBasedAssumptionForecastService.GetInputsFromTradingHistory(portfolioSize, positionSizePercent, desiredReturnPercent, executions);
                var outputsDto = _mapper.Map<DesiredPerformanceResultsDto>(_resultsBasedAssumptionForecastService.CalculateForecast(inputs)  );
                return Ok(outputsDto);
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
