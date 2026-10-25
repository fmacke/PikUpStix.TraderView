using Microsoft.AspNetCore.Mvc;
using traderview.Server.DTOs;
using traderview.Server.Services;
using TraderView.Application.Interfaces.Services;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/tradeviewer")]
    public class TradeViewerController : ControllerBase
    {
        private readonly ITradeViewerService _tradeViewerService;
        private readonly IReportRunnerService _reportRunnerService;
        private readonly ILogger<TradeViewerController> _logger;
        private readonly IPositionService _positionService;

        public TradeViewerController(
            ITradeViewerService tradeViewerService,
            IReportRunnerService reportRunnerService,
            IPositionService positionService,
            ILogger<TradeViewerController> logger)
        {
            _tradeViewerService = tradeViewerService;
            _reportRunnerService = reportRunnerService;
            _positionService = positionService;
            _logger = logger;
        }

        /// <summary>
        /// Get all trades
        /// </summary>
        /// <returns>List of all trades</returns>
        [HttpGet("trades")]
        [ProducesResponseType(typeof(List<TradeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<TradeDto>>> GetAllTradesAsync()
        {
            try
            {
                var positions = await _positionService.GetAllPositionsAsync(true);
                List<TradeDto> tradeDtos = new List<TradeDto>();
                foreach (var p in positions)
                {
                    var averageCostBasis = 0m;
                    var averageSalePrice = 0m;
                    var quantitySum = 0m;
                    var quantitySoldSum = 0m;
                    var totalSale = 0m;
                    var totalCost = 0m;
                    var positionTotalSize = 0m;
                    var postionDirection = p.TradeExecutions.OrderBy(x => x.Id).FirstOrDefault()?.BuySell ?? "BUY";
                    var fifoPnlRealized = 0m;
                    var executions = new List<TradeExecutionDto>();
                    foreach (var te in p.TradeExecutions.OrderBy(x => x.Id))
                    {
                        _logger.LogInformation("TradeExecution ID: {TradeExecutionId}, Position ID: {PositionId}, Execution Type: {ExecutionType}, Price: {Price}, Quantity: {Quantity}, Date: {Date}",
                            te.Id, p.Id, te.BuySell, te.ClosePrice, te.Quantity, te.DateTime);
                        if (postionDirection == "BUY" && te.BuySell == "BUY" || postionDirection == "SELL" && te.BuySell == "SELL") 
                        {
                            quantitySum += Convert.ToDecimal(te.Quantity);
                            totalCost += (Convert.ToDecimal(te.TradePrice) * Convert.ToDecimal(te.Quantity)) + Convert.ToDecimal(te.IbCommission);
                            positionTotalSize += Convert.ToDecimal(te.Quantity);
                        }
                        if(postionDirection == "BUY" && te.BuySell == "SELL" || postionDirection == "SELL" && te.BuySell == "BUY") 
                        {
                            quantitySoldSum += Convert.ToDecimal(te.Quantity);
                            totalSale += (Convert.ToDecimal(te.TradePrice) * Convert.ToDecimal(te.Quantity)) + Convert.ToDecimal(te.IbCommission);
                        }
                        fifoPnlRealized += (Convert.ToDecimal(te.FifoPnlRealized));
                        executions.Add(new TradeExecutionDto
                        {
                            Id = te.Id,
                            PositionId = Convert.ToInt32(te.PositionId),
                            Symbol = te.Symbol,
                            TradeId = te.TradeId,
                            TradeDate = te.DateTime,
                            Quantity = te.Quantity,
                            TradePrice = te.TradePrice,
                            BuySell = te.BuySell,
                            FifoPnlRealized = te.FifoPnlRealized,
                            IbCommission = te.IbCommission,
                            IbCommissionCurrency = te.IbCommissionCurrency,
                            ClosePrice = te.ClosePrice,
                            Cost = te.Cost,
                            IbExecId = te.IbExecId
                        });
                    }
                    averageCostBasis = totalCost / (quantitySum != 0 ? quantitySum : 1);
                    averageSalePrice = totalSale / (quantitySoldSum != 0 ? quantitySoldSum : 1);

                    var tradeDto = new TradeDto
                    {
                        PositionId = p.Id,
                        InstrumentId = p.InstrumentId,
                        Symbol = p.Instrument.DataName,
                        EntryDate = p.OpenDate,
                        ExitDate = p.CloseDate ?? DateTime.MinValue,
                        EntryPrice = averageCostBasis,
                        ExitPrice = averageSalePrice,
                        Quantity = positionTotalSize,
                        Pnl = fifoPnlRealized,
                        BuySell = postionDirection,
                        Executions = executions
                    };
                    tradeDtos.Add(tradeDto);
                }
                foreach (var position in positions)
                {
                    _logger.LogInformation("Position ID: {PositionId}, Symbol: {Symbol}, Entry Date: {EntryDate}, Exit Date: {ExitDate}",
                        position.Id, position.Instrument.DataName, position.OpenDate, position.CloseDate);
                }
                return Ok(tradeDtos);
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

        /// <summary>
        /// Get candlestick data for a specific trade
        /// </summary>
        /// <param name="positionId">The trade ID</param>
        /// <param name="daysBefore">Number of calendar days before trade entry to include (default: 150, which typically provides ~100 trading days)</param>
        /// <param name="daysAfter">Number of calendar days after trade exit to include (default: 150, which typically provides ~100 trading days)</param>
        /// <returns>TradeExecution context with candlestick data</returns>
        [HttpGet("trades/{positionId}/candlesticks")]
        [ProducesResponseType(typeof(TradeContextDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TradeContextDto>> GetTradeCandlesticksAsync(
            int positionId,
            [FromQuery] int daysBefore = 150,
            [FromQuery] int daysAfter = 150)
        {
            try
            {
                _logger.LogInformation("Fetching candlesticks for trade {positionId}", positionId);

                var tradeContext = await _tradeViewerService.GetTradeContextAsync(positionId, daysBefore, daysAfter);

                if (tradeContext == null)
                {
                    _logger.LogWarning("Position {positionId} not found", positionId);
                    return NotFound(new { message = $"Position with ID {positionId} not found" });
                }

                _logger.LogInformation("Found {Count} candlesticks for position {positionId}", 
                    tradeContext.Candlesticks.Count, positionId);

                return Ok(tradeContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching candlesticks for position {positionId}", positionId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching candlestick data", detail = ex.Message }
                );
            }
        }

        /// <summary>
        /// Sync IBKR data - fetches reports from Interactive Brokers and updates database
        /// </summary>
        /// <returns>Success message if sync completes successfully</returns>
        [HttpPost("sync")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> SyncIBKRDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting IBKR data sync...");

                await _reportRunnerService.RunReportAsync(false, true);

                _logger.LogInformation("IBKR data sync completed successfully");
                return Ok(new { message = "IBKR data sync completed successfully", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during IBKR data sync");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error during IBKR data sync", detail = ex.Message }
                );
            }
        }
    }
}
