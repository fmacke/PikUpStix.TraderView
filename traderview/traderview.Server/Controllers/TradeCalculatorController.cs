using Microsoft.AspNetCore.Mvc;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Models;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TradeCalculatorController : ControllerBase
    {
        private readonly ITradeCalculatorService _calculatorService;
        private readonly ILogger<TradeCalculatorController> _logger;

        public TradeCalculatorController(ILogger<TradeCalculatorController> logger, ITradeCalculatorService calculatorService)
        {
            _logger = logger;
            _calculatorService = calculatorService;
        }

        [HttpPost("calculate")]
        public ActionResult<TradeCalculationResponse> Calculate([FromBody] TradeCalculationRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid calculation request.");
            }

            var result = _calculatorService.CalculatePosition(request);
            return Ok(result);
        }
    }
}
