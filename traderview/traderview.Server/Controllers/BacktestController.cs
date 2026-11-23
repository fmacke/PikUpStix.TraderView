using Microsoft.AspNetCore.Mvc;
using TraderView.Application.Strategies.Backtest;
using TraderView.Application.UseCases.Backtest;
using TraderView.Domain.Backtest;
using TraderView.Infrastructure.DbContexts;
using TraderView.Infrastructure.Services.Backtest;

namespace traderview.Server.Controllers
{
    /// <summary>
    /// API endpoint for running backtests on historical data.
    /// 
    /// POST /api/backtests - Run a backtest and return results with equity curve and trade log.
    /// </summary>
    [ApiController]
    [Route("api/backtests")]
    public class BacktestController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<BacktestController> _logger;

        public BacktestController(AppDbContext dbContext, ILogger<BacktestController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Run a complete backtest and return results.
        /// 
        /// Request body example:
        /// {
        ///   "instrumentId": 1,
        ///   "startDate": "2023-01-01",
        ///   "endDate": "2024-12-31",
        ///   "initialCapital": 10000,
        ///   "strategyType": "SMACrossover",
        ///   "warmupBars": 50
        /// }
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BacktestResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BacktestResult>> RunBacktest([FromBody] BacktestRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Starting backtest for instrument {InstrumentId} from {StartDate} to {EndDate} using {Strategy}",
                    request.InstrumentId,
                    request.StartDate,
                    request.EndDate,
                    request.StrategyType);

                // Validate input
                if (request.InstrumentId <= 0)
                    return BadRequest(new { message = "Invalid instrumentId" });

                if (request.StartDate >= request.EndDate)
                    return BadRequest(new { message = "StartDate must be before EndDate" });

                if (request.InitialCapital <= 0)
                    return BadRequest(new { message = "InitialCapital must be positive" });

                // Instantiate the appropriate strategy
                ITradingStrategy strategy = request.StrategyType?.ToLower() switch
                {
                    "smacrossover" => new SMACrossoverStrategy(
                        request.FastPeriod ?? 12,
                        request.SlowPeriod ?? 26,
                        request.AtrPeriod ?? 14),
                    "rsioversold" => new RSIOversoldStrategy(
                        request.RsiPeriod ?? 14,
                        request.AtrPeriod ?? 14),
                    _ => new SMACrossoverStrategy(), // Default
                };

                // Create data provider and execution simulator
                var barProvider = new HistoricalBarProvider(_dbContext);
                var executionSimulator = new ExecutionSimulator(
                    slippagePercent: request.SlippagePercent ?? 0.01,
                    commissionPerTrade: request.CommissionPerTrade ?? 10.0);

                // Run backtest
                var runner = new BacktestRunner(strategy, barProvider, executionSimulator);
                var result = await runner.RunBacktestAsync(
                    request.InstrumentId,
                    request.StartDate,
                    request.EndDate,
                    request.InitialCapital,
                    request.WarmupBars ?? 50);

                _logger.LogInformation(
                    "Backtest completed: {TotalTrades} trades, {WinRate:P} win rate, {TotalReturn:P} return",
                    result.TotalTrades,
                    result.WinRate,
                    result.TotalReturnPercent / 100);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Backtest validation error");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running backtest");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error running backtest", detail = ex.Message });
            }
        }
    }

    /// <summary>
    /// Request DTO for backtest execution.
    /// </summary>
    public class BacktestRequest
    {
        /// <summary>
        /// Instrument ID to backtest
        /// </summary>
        public int InstrumentId { get; set; }

        /// <summary>
        /// Start date (inclusive)
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date (inclusive)
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Starting capital in dollars
        /// </summary>
        public double InitialCapital { get; set; } = 10000;

        /// <summary>
        /// Strategy to use: "SMACrossover", "RSIOversold", etc.
        /// </summary>
        public string? StrategyType { get; set; } = "SMACrossover";

        /// <summary>
        /// Number of bars to warm up before trading
        /// </summary>
        public int? WarmupBars { get; set; } = 50;

        /// <summary>
        /// Slippage percentage (e.g., 0.01 = 0.01%)
        /// </summary>
        public double? SlippagePercent { get; set; } = 0.01;

        /// <summary>
        /// Commission per trade in dollars
        /// </summary>
        public double? CommissionPerTrade { get; set; } = 10.0;

        // SMA Crossover parameters
        public int? FastPeriod { get; set; } = 12;
        public int? SlowPeriod { get; set; } = 26;

        // RSI and ATR parameters
        public int? RsiPeriod { get; set; } = 14;
        public int? AtrPeriod { get; set; } = 14;
    }
}
