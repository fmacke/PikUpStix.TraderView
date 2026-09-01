using Microsoft.AspNetCore.Mvc;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities.FMP;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/stockscreener")]
    public class StockScreenerController : ControllerBase
    {
        private readonly ILogger<StockScreenerController> _logger;
        private readonly IMarketDataService _marketDataService;
        private readonly ICanSlimScreenerService _canSlimScreenerService;

        public StockScreenerController(
            ILogger<StockScreenerController> logger,
            IMarketDataService marketDataService,
            ICanSlimScreenerService canSlimScreenerService)
        {
            _logger = logger;
            _marketDataService = marketDataService;
            _canSlimScreenerService = canSlimScreenerService;
        }

        /// <summary>
        /// Get all open positions
        /// </summary>
        /// <returns>List of all open positions</returns>
        [HttpGet("EvaluateCurrentQuarterEpsAsync/{symbol}")]
        [ProducesResponseType(typeof(CanSlimCurrentQuarterMetric), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CanSlimCurrentQuarterMetric>> EvaluateCurrentQuarterEpsAsync(string symbol)
        {
            try
            {
                _logger.LogInformation("Fetching all qualifying EPS stocks");
                var epsReport = await _marketDataService.EvaluateCurrentQuarterEpsAsync(symbol, 25M, 20M);
                return Ok(epsReport);
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

        /// <summary>
        /// Run the stock screener to get a list of qualifying CAN SLIM candidates
        /// </summary>
        /// <param name="symbol">The stock symbol to screen for CAN SLIM candidates</param>
        /// <returns>A list of qualifying CAN SLIM candidates</returns>
        [HttpPost("RunStockScreener")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> RunStockScreener()
        {
            try
            {
                _logger.LogInformation("Fetching all qualifying CAN SLIM candidates");
                var stocksShortList = await _marketDataService.RunScreenerAsync(new CanSlimScreenerCriteria());
                _logger.LogInformation("IBKR data sync completed successfully");
                return Ok(new { message = "IBKR data sync completed successfully", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {       
                _logger.LogError(ex, "Error fetching CAN SLIM candidates")      ;
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching CAN SLIM candidates", detail = ex.Message }
                );
            }
        }
        /// <summary>
        /// Run the stock screener to get a list of qualifying CAN SLIM candidates
        /// </summary>
        /// <param name="symbol">The stock symbol to screen for CAN SLIM candidates</param>
        /// <returns>A list of qualifying CAN SLIM candidates</returns>
        [HttpGet("GetCanSlimCandidates")]
        [ProducesResponseType(typeof(IReadOnlyList<CanSlimCandidate>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyList<CanSlimCandidate>>> GetCanSlimCandidates()
        {
            try
            {
                _logger.LogInformation("Fetching all qualifying CAN SLIM candidates");
                var stocksShortList = await _marketDataService.RunScreenerAsync(new CanSlimScreenerCriteria());
                return Ok(stocksShortList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching CAN SLIM candidates");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching CAN SLIM candidates", detail = ex.Message }
                );
            }
        }
        /// <summary>
        /// Run the stock screener to get a list of qualifying CAN SLIM candidates
        /// </summary>
        /// <param name="symbol">The stock symbol to screen for CAN SLIM candidates</param>
        /// <returns>A list of qualifying CAN SLIM candidates</returns>
        [HttpGet("AddCanSlimCandidate/")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> AddCanSlimCandidate()
        {
            try
            {
                _logger.LogInformation("Adding a new CAN SLIM candidate");

                var canSlimScreenerSnapShotId = await _canSlimScreenerService.CreateCanSlimScreenerSnapshot(GetDummList());
                return Ok(canSlimScreenerSnapShotId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding CAN SLIM candidate");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error adding CAN SLIM candidate", detail = ex.Message }
                );
            }
        }
        public static List<CanSlimCandidate> GetDummList()
        {
            return new List<CanSlimCandidate>
            {
                // 1. NVDA - High Growth Leader
                new CanSlimCandidate
                {
                    Id = 1,
                    CanSlimScreenerSnapshotId = 101,
                    Symbol = "NVDA",
                    Exchange = "NASDAQ",
                    CompanyName = "NVIDIA Corporation",
                    Sector = "Technology",
                    Industry = "Semiconductors",
                    PassesBoth = true,
                    Price = 125.50m,
                    Volume = 48500000m,
                    MarketCap = 3080000000000m,
                    CurrentQuarterLatestQuarterDate = "Q2-2026",
                    CurrentQuarterEpsGrowthYoYpercent = 88.5m,
                    CurrentQuarterRevenueGrowthYoYpercent = 112.0m,
                    CurrentQuarterIsAccelerating = true,
                    CurrentQuarterPassesCriteria = true,
                    EvaluationDateUtc = DateTime.UtcNow,
                    AnnualEpsCagr3YearPercent = 74.2m,
                    AnnualEpsCagr5YearPercent = 58.4m,
                    AnnualReturnOnEquityPercent = 62.3m,
                    AnnualHasConsecutiveAnnualGrowth = true,
                    AnnualLatestFiscalYear = "2025",
                    AnnualLatestFiscalYearEps = 2.85m,
                    AnnualPriorYear1Eps = 1.19m,
                    AnnualPriorYear2Eps = 0.34m,
                    AnnualPriorYear3Eps = 0.26m,
                    AnnualOperatingMarginPercent = 61.5m,
                    AnnualReturnOnAssetsPercent = 38.2m,
                    AnnualPassesCriteria = true,
                    AnnualFundamentalGrade = "A+",
                    CanSlimCandidateAnnualHistories = new List<CanSlimCandidateAnnualHistory>
                    {
                        new CanSlimCandidateAnnualHistory
                        {
                            CandidateId = 1,
                            CalendarYear = "2025",
                            FiscalDate = "2025-01-26",
                            Revenue = 60922000000m,
                            NetIncome = 29760000000m,
                            EpsDiluted = 2.85m,
                            EpsGrowthYoYpercent = 139.5m
                        },
                        new CanSlimCandidateAnnualHistory
                        {
                            CandidateId = 1,
                            CalendarYear = "2024",
                            FiscalDate = "2024-01-28",
                            Revenue = 26974000000m,
                            NetIncome = 4368000000m,
                            EpsDiluted = 1.19m,
                            EpsGrowthYoYpercent = 250.0m
                        }
                    }
                },

                // 2. PLTR - Software & Analytics Expansion
                new CanSlimCandidate
                {
                    Id = 2,
                    CanSlimScreenerSnapshotId = 101,
                    Symbol = "PLTR",
                    Exchange = "NYSE",
                    CompanyName = "Palantir Technologies Inc.",
                    Sector = "Technology",
                    Industry = "Software - Infrastructure",
                    PassesBoth = true,
                    Price = 34.20m,
                    Volume = 31200000m,
                    MarketCap = 76000000000m,
                    CurrentQuarterLatestQuarterDate = "Q2-2026",
                    CurrentQuarterEpsGrowthYoYpercent = 60.0m,
                    CurrentQuarterRevenueGrowthYoYpercent = 27.2m,
                    CurrentQuarterIsAccelerating = true,
                    CurrentQuarterPassesCriteria = true,
                    EvaluationDateUtc = DateTime.UtcNow,
                    AnnualEpsCagr3YearPercent = 48.6m,
                    AnnualEpsCagr5YearPercent = null,
                    AnnualHasConsecutiveAnnualGrowth = true,
                    AnnualLatestFiscalYear = "2025",
                    AnnualLatestFiscalYearEps = 0.38m,
                    AnnualPriorYear1Eps = 0.25m,
                    AnnualPriorYear2Eps = 0.16m,
                    AnnualPriorYear3Eps = -0.05m,
                    AnnualOperatingMarginPercent = 24.1m,
                    AnnualReturnOnAssetsPercent = 14.8m,
                    AnnualPassesCriteria = true,
                    AnnualFundamentalGrade = "A",
                    CanSlimCandidateAnnualHistories = new List<CanSlimCandidateAnnualHistory>
                    {
                        new CanSlimCandidateAnnualHistory
                        {
                        CandidateId = 2,
                        CalendarYear = "2025",
                            FiscalDate = "2025-12-31",
                            Revenue = 2860000000m,
                            NetIncome = 850000000m,
                            EpsDiluted = 0.38m,
                            EpsGrowthYoYpercent = 52.0m
                        },
                        new CanSlimCandidateAnnualHistory
                        {
                            CandidateId = 2,
                            CalendarYear = "2024",
                            FiscalDate = "2024-12-31",
                            Revenue = 2225000000m,
                            NetIncome = 512000000m,
                            EpsDiluted = 0.25m,
                            EpsGrowthYoYpercent = 56.3m
                        }
                    }
                },

                // 3. CELH - Consumer Growth Leader
                new CanSlimCandidate
                {
                    Id = 3,
                    CanSlimScreenerSnapshotId = 101,
                    Symbol = "CELH",
                    Exchange = "NASDAQ",
                    CompanyName = "Celsius Holdings, Inc.",
                    Sector = "Consumer Defensive",
                    Industry = "Beverages - Non-Alcoholic",
                    PassesBoth = true,
                    Price = 58.75m,
                    Volume = 8400000m,
                    MarketCap = 13800000000m,
                    CurrentQuarterLatestQuarterDate = "Q2-2026",
                    CurrentQuarterEpsGrowthYoYpercent = 38.2m,
                    CurrentQuarterRevenueGrowthYoYpercent = 35.1m,
                    CurrentQuarterIsAccelerating = false,
                    CurrentQuarterPassesCriteria = true,
                    AnnualEpsCagr3YearPercent = 52.1m,
                    AnnualEpsCagr5YearPercent = 41.8m,
                    AnnualReturnOnEquityPercent = 29.7m,
                    AnnualHasConsecutiveAnnualGrowth = true,
                    AnnualLatestFiscalYear = "2025",
                    AnnualLatestFiscalYearEps = 1.12m,
                    AnnualPriorYear1Eps = 0.77m,
                    AnnualPriorYear2Eps = 0.45m,
                    AnnualPriorYear3Eps = 0.18m,
                    AnnualOperatingMarginPercent = 21.3m,
                    AnnualReturnOnAssetsPercent = 18.6m,
                    AnnualPassesCriteria = true,
                    AnnualFundamentalGrade = "A",
                    CanSlimCandidateAnnualHistories = new List<CanSlimCandidateAnnualHistory>
                    {
                        new CanSlimCandidateAnnualHistory
                        {
                            CandidateId = 3,
                            CalendarYear = "2025",
                            FiscalDate = "2025-12-31",
                            Revenue = 1540000000m,
                            NetIncome = 265000000m,
                            EpsDiluted = 1.12m,
                            EpsGrowthYoYpercent = 45.5m
                        },
                        new CanSlimCandidateAnnualHistory
                        {
                            CandidateId = 3,
                            CalendarYear = "2024",
                            FiscalDate = "2024-12-31",
                            Revenue = 1318000000m,
                            NetIncome = 182000000m,
                            EpsDiluted = 0.77m,
                            EpsGrowthYoYpercent = 71.1m
                        }
                    }
                },

                // 4. ANET - Networking & Cloud Infrastructure
                new CanSlimCandidate
                {
                    Id = 4,
                    CanSlimScreenerSnapshotId = 101,
                    Symbol = "ANET",
                    Exchange = "NYSE",
                    CompanyName = "Arista Networks, Inc.",
                    Sector = "Technology",
                    Industry = "Computer Hardware",
                    PassesBoth = true,
                    Price = 312.40m,
                    Volume = 2600000m,
                    MarketCap = 98000000000m,
                    CurrentQuarterLatestQuarterDate = "Q2-2026",
                    CurrentQuarterEpsGrowthYoYpercent = 32.5m,
                    CurrentQuarterRevenueGrowthYoYpercent = 28.9m,
                    CurrentQuarterIsAccelerating = false,
                    CurrentQuarterPassesCriteria = true,
                    EvaluationDateUtc = DateTime.UtcNow,
                    AnnualEpsCagr3YearPercent = 36.8m,
                    AnnualEpsCagr5YearPercent = 30.1m,
                    AnnualReturnOnEquityPercent = 33.2m,
                    AnnualHasConsecutiveAnnualGrowth = true,
                    AnnualLatestFiscalYear = "2025",
                    AnnualLatestFiscalYearEps = 8.42m,
                    AnnualPriorYear1Eps = 6.24m,
                    AnnualPriorYear2Eps = 4.58m,
                    AnnualPriorYear3Eps = 2.87m,
                    AnnualOperatingMarginPercent = 42.1m,
                    AnnualReturnOnAssetsPercent = 24.5m,
                    AnnualPassesCriteria = true,
                    AnnualFundamentalGrade = "A",
                    CanSlimCandidateAnnualHistories = new List<CanSlimCandidateAnnualHistory>
                    {
                        new CanSlimCandidateAnnualHistory
                        {
                                CandidateId = 4,
                                CalendarYear = "2025",
                                FiscalDate = "2025-12-31",
                                Revenue = 7150000000m,
                                NetIncome = 2680000000m,
                                EpsDiluted = 8.42m,
                                EpsGrowthYoYpercent = 34.9m
                        },
                        new CanSlimCandidateAnnualHistory
                        {
                            CandidateId = 4,
                            CalendarYear = "2024",
                            FiscalDate = "2024-12-31",
                            Revenue = 5860000000m,
                            NetIncome = 1980000000m,
                            EpsDiluted = 6.24m,
                            EpsGrowthYoYpercent = 36.2m
                        }
                    }
                },

                // 5. XYZ - Failing Criteria Example (For filtering/testing)
                new CanSlimCandidate
                {
                    Id = 5,
                    CanSlimScreenerSnapshotId = 101,
                    Symbol = "XYZ",
                    Exchange = "NYSE",
                    CompanyName = "XYZ Industrial Group",
                    Sector = "Industrials",
                    Industry = "Specialty Industrial Machinery",
                    PassesBoth = false,
                    Price = 42.10m,
                    Volume = 1200000m,
                    MarketCap = 5400000000m,
                    CurrentQuarterLatestQuarterDate = "Q2-2026",
                    CurrentQuarterEpsGrowthYoYpercent = 8.2m,
                    CurrentQuarterRevenueGrowthYoYpercent = 4.0m,
                    CurrentQuarterIsAccelerating = false,
                    CurrentQuarterPassesCriteria = false,
                    EvaluationDateUtc = DateTime.UtcNow,
                    AnnualEpsCagr3YearPercent = 11.4m,
                    AnnualEpsCagr5YearPercent = 9.8m,
                    AnnualReturnOnEquityPercent = 12.1m,
                    AnnualHasConsecutiveAnnualGrowth = false,
                    AnnualLatestFiscalYear = "2025",
                    AnnualLatestFiscalYearEps = 2.10m,
                    AnnualPriorYear1Eps = 2.30m,
                    AnnualPriorYear2Eps = 1.95m,
                    AnnualPriorYear3Eps = 1.80m,
                    AnnualOperatingMarginPercent = 11.2m,
                    AnnualReturnOnAssetsPercent = 6.4m,
                    AnnualPassesCriteria = false,
                    AnnualFundamentalGrade = "C",
                    CanSlimCandidateAnnualHistories = new List<CanSlimCandidateAnnualHistory>
                    {
                        new CanSlimCandidateAnnualHistory
                        {
                                CandidateId = 5,
                                CalendarYear = "2025",
                                FiscalDate = "2025-12-31",
                                Revenue = 2100000000m,
                                NetIncome = 240000000m,
                                EpsDiluted = 2.10m,
                                EpsGrowthYoYpercent = -8.7m
                        },
                        new CanSlimCandidateAnnualHistory   
                        {
                            CandidateId = 5,
                            CalendarYear = "2024",
                            FiscalDate = "2024-12-31",
                            Revenue = 2050000000m,
                            NetIncome = 265000000m,
                            EpsDiluted = 2.30m,
                            EpsGrowthYoYpercent = 17.9m
                        }
                    }
                }
            };
        }
    }
}

