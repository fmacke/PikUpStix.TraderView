using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Mvc;
using PikUpStix.TraderView.Services;
using traderview.Server.DTOs;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Models.FMP;
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
                    CanSlimScreenerSnapShotId = 101,
                    Symbol = "NVDA",
                    Exchange = "NASDAQ",
                    CompanyName = "NVIDIA Corporation",
                    Sector = "Technology",
                    Industry = "Semiconductors",
                    PassesBoth = true,
                    Price = 125.50m,
                    Volume = 48500000m,
                    MarketCap = 3080000000000m,
                    CurrentQuarter = new CanSlimCurrentQuarterMetric
                    {
                        Symbol = "NVDA",
                        LatestQuarterDate = "Q2-2026",
                        EpsGrowthYoYPercent = 88.5m,
                        RevenueGrowthYoYPercent = 112.0m,
                        IsAccelerating = true,
                        PassesCriteria = true
                    },
                    Annual = new CanSlimAnnualMetric
                    {
                        Symbol = "NVDA",
                        EvaluationDateUtc = DateTime.UtcNow,
                        EpsCagr3YearPercent = 74.2m,
                        EpsCagr5YearPercent = 58.4m,
                        ReturnOnEquityPercent = 62.3m,
                        HasConsecutiveAnnualGrowth = true,
                        LatestFiscalYear = "2025",
                        LatestFiscalYearEps = 2.85m,
                        PriorYear1Eps = 1.19m,
                        PriorYear2Eps = 0.34m,
                        PriorYear3Eps = 0.26m,
                        OperatingMarginPercent = 61.5m,
                        ReturnOnAssetsPercent = 38.2m,
                        PassesCriteria = true,
                        FundamentalGrade = "A+",
                        AnnualHistory = new List<AnnualEarningsPoint>
                        {
                            new AnnualEarningsPoint
                            {
                                CandidateId = 1,
                                CalendarYear = "2025",
                                FiscalDate = "2025-01-26",
                                Revenue = 60922000000m,
                                NetIncome = 29760000000m,
                                EpsDiluted = 2.85m,
                                EpsGrowthYoYPercent = 139.5m
                            },
                            new AnnualEarningsPoint
                            {
                                CandidateId = 1,
                                CalendarYear = "2024",
                                FiscalDate = "2024-01-28",
                                Revenue = 26974000000m,
                                NetIncome = 4368000000m,
                                EpsDiluted = 1.19m,
                                EpsGrowthYoYPercent = 250.0m
                            }
                        }
                    }
                },

                // 2. PLTR - Software & Analytics Expansion
                new CanSlimCandidate
                {
                    Id = 2,
                    CanSlimScreenerSnapShotId = 101,
                    Symbol = "PLTR",
                    Exchange = "NYSE",
                    CompanyName = "Palantir Technologies Inc.",
                    Sector = "Technology",
                    Industry = "Software - Infrastructure",
                    PassesBoth = true,
                    Price = 34.20m,
                    Volume = 31200000m,
                    MarketCap = 76000000000m,
                    CurrentQuarter = new CanSlimCurrentQuarterMetric
                    {
                        Symbol = "PLTR",
                        LatestQuarterDate = "Q2-2026",
                        EpsGrowthYoYPercent = 60.0m,
                        RevenueGrowthYoYPercent = 27.2m,
                        IsAccelerating = true,
                        PassesCriteria = true
                    },
                    Annual = new CanSlimAnnualMetric
                    {
                        Symbol = "PLTR",
                        EvaluationDateUtc = DateTime.UtcNow,
                        EpsCagr3YearPercent = 48.6m,
                        EpsCagr5YearPercent = null,
                        ReturnOnEquityPercent = 19.4m,
                        HasConsecutiveAnnualGrowth = true,
                        LatestFiscalYear = "2025",
                        LatestFiscalYearEps = 0.38m,
                        PriorYear1Eps = 0.25m,
                        PriorYear2Eps = 0.16m,
                        PriorYear3Eps = -0.05m,
                        OperatingMarginPercent = 24.1m,
                        ReturnOnAssetsPercent = 14.8m,
                        PassesCriteria = true,
                        FundamentalGrade = "A",
                        AnnualHistory = new List<AnnualEarningsPoint>
                        {
                            new AnnualEarningsPoint
                            {
                                CandidateId = 2,
                                CalendarYear = "2025",
                                FiscalDate = "2025-12-31",
                                Revenue = 2860000000m,
                                NetIncome = 850000000m,
                                EpsDiluted = 0.38m,
                                EpsGrowthYoYPercent = 52.0m
                            },
                            new AnnualEarningsPoint
                            {
                                CandidateId = 2,
                                CalendarYear = "2024",
                                FiscalDate = "2024-12-31",
                                Revenue = 2225000000m,
                                NetIncome = 512000000m,
                                EpsDiluted = 0.25m,
                                EpsGrowthYoYPercent = 56.3m
                            }
                        }
                    }
                },

                // 3. CELH - Consumer Growth Leader
                new CanSlimCandidate
                {
                    Id = 3,
                    CanSlimScreenerSnapShotId = 101,
                    Symbol = "CELH",
                    Exchange = "NASDAQ",
                    CompanyName = "Celsius Holdings, Inc.",
                    Sector = "Consumer Defensive",
                    Industry = "Beverages - Non-Alcoholic",
                    PassesBoth = true,
                    Price = 58.75m,
                    Volume = 8400000m,
                    MarketCap = 13800000000m,
                    CurrentQuarter = new CanSlimCurrentQuarterMetric
                    {
                        Symbol = "CELH",
                        LatestQuarterDate = "Q2-2026",
                        EpsGrowthYoYPercent = 38.2m,
                        RevenueGrowthYoYPercent = 35.1m,
                        IsAccelerating = false,
                        PassesCriteria = true
                    },
                    Annual = new CanSlimAnnualMetric
                    {
                        Symbol = "CELH",
                        EvaluationDateUtc = DateTime.UtcNow,
                        EpsCagr3YearPercent = 52.1m,
                        EpsCagr5YearPercent = 41.8m,
                        ReturnOnEquityPercent = 29.7m,
                        HasConsecutiveAnnualGrowth = true,
                        LatestFiscalYear = "2025",
                        LatestFiscalYearEps = 1.12m,
                        PriorYear1Eps = 0.77m,
                        PriorYear2Eps = 0.45m,
                        PriorYear3Eps = 0.18m,
                        OperatingMarginPercent = 21.3m,
                        ReturnOnAssetsPercent = 18.6m,
                        PassesCriteria = true,
                        FundamentalGrade = "A",
                        AnnualHistory = new List<AnnualEarningsPoint>
                        {
                            new AnnualEarningsPoint
                            {
                                CandidateId = 3,
                                CalendarYear = "2025",
                                FiscalDate = "2025-12-31",
                                Revenue = 1540000000m,
                                NetIncome = 265000000m,
                                EpsDiluted = 1.12m,
                                EpsGrowthYoYPercent = 45.5m
                            },
                            new AnnualEarningsPoint
                            {
                                CandidateId = 3,
                                CalendarYear = "2024",
                                FiscalDate = "2024-12-31",
                                Revenue = 1318000000m,
                                NetIncome = 182000000m,
                                EpsDiluted = 0.77m,
                                EpsGrowthYoYPercent = 71.1m
                            }
                        }
                    }
                },

                // 4. ANET - Networking & Cloud Infrastructure
                new CanSlimCandidate
                {
                    Id = 4,
                    CanSlimScreenerSnapShotId = 101,
                    Symbol = "ANET",
                    Exchange = "NYSE",
                    CompanyName = "Arista Networks, Inc.",
                    Sector = "Technology",
                    Industry = "Computer Hardware",
                    PassesBoth = true,
                    Price = 312.40m,
                    Volume = 2600000m,
                    MarketCap = 98000000000m,
                    CurrentQuarter = new CanSlimCurrentQuarterMetric
                    {
                        Symbol = "ANET",
                        LatestQuarterDate = "Q2-2026",
                        EpsGrowthYoYPercent = 32.5m,
                        RevenueGrowthYoYPercent = 28.9m,
                        IsAccelerating = false,
                        PassesCriteria = true
                    },
                    Annual = new CanSlimAnnualMetric
                    {
                        Symbol = "ANET",
                        EvaluationDateUtc = DateTime.UtcNow,
                        EpsCagr3YearPercent = 36.8m,
                        EpsCagr5YearPercent = 30.1m,
                        ReturnOnEquityPercent = 33.2m,
                        HasConsecutiveAnnualGrowth = true,
                        LatestFiscalYear = "2025",
                        LatestFiscalYearEps = 8.42m,
                        PriorYear1Eps = 6.24m,
                        PriorYear2Eps = 4.58m,
                        PriorYear3Eps = 2.87m,
                        OperatingMarginPercent = 42.1m,
                        ReturnOnAssetsPercent = 24.5m,
                        PassesCriteria = true,
                        FundamentalGrade = "A",
                        AnnualHistory = new List<AnnualEarningsPoint>
                        {
                            new AnnualEarningsPoint
                            {
                                CandidateId = 4,
                                CalendarYear = "2025",
                                FiscalDate = "2025-12-31",
                                Revenue = 7150000000m,
                                NetIncome = 2680000000m,
                                EpsDiluted = 8.42m,
                                EpsGrowthYoYPercent = 34.9m
                            },
                            new AnnualEarningsPoint
                            {
                                CandidateId = 4,
                                CalendarYear = "2024",
                                FiscalDate = "2024-12-31",
                                Revenue = 5860000000m,
                                NetIncome = 1980000000m,
                                EpsDiluted = 6.24m,
                                EpsGrowthYoYPercent = 36.2m
                            }
                        }
                    }
                },

                // 5. XYZ - Failing Criteria Example (For filtering/testing)
                new CanSlimCandidate
                {
                    Id = 5,
                    CanSlimScreenerSnapShotId = 101,
                    Symbol = "XYZ",
                    Exchange = "NYSE",
                    CompanyName = "XYZ Industrial Group",
                    Sector = "Industrials",
                    Industry = "Specialty Industrial Machinery",
                    PassesBoth = false,
                    Price = 42.10m,
                    Volume = 1200000m,
                    MarketCap = 5400000000m,
                    CurrentQuarter = new CanSlimCurrentQuarterMetric
                    {
                        Symbol = "XYZ",
                        LatestQuarterDate = "Q2-2026",
                        EpsGrowthYoYPercent = 8.2m,
                        RevenueGrowthYoYPercent = 4.0m,
                        IsAccelerating = false,
                        PassesCriteria = false
                    },
                    Annual = new CanSlimAnnualMetric
                    {
                        Symbol = "XYZ",
                        EvaluationDateUtc = DateTime.UtcNow,
                        EpsCagr3YearPercent = 11.4m,
                        EpsCagr5YearPercent = 9.8m,
                        ReturnOnEquityPercent = 12.1m,
                        HasConsecutiveAnnualGrowth = false,
                        LatestFiscalYear = "2025",
                        LatestFiscalYearEps = 2.10m,
                        PriorYear1Eps = 2.30m,
                        PriorYear2Eps = 1.95m,
                        PriorYear3Eps = 1.80m,
                        OperatingMarginPercent = 11.2m,
                        ReturnOnAssetsPercent = 6.4m,
                        PassesCriteria = false,
                        FundamentalGrade = "C",
                        AnnualHistory = new List<AnnualEarningsPoint>
                        {
                            new AnnualEarningsPoint
                            {
                                CandidateId = 5,
                                CalendarYear = "2025",
                                FiscalDate = "2025-12-31",
                                Revenue = 2100000000m,
                                NetIncome = 240000000m,
                                EpsDiluted = 2.10m,
                                EpsGrowthYoYPercent = -8.7m
                            },
                            new AnnualEarningsPoint
                            {
                                CandidateId = 5,
                                CalendarYear = "2024",
                                FiscalDate = "2024-12-31",
                                Revenue = 2050000000m,
                                NetIncome = 265000000m,
                                EpsDiluted = 2.30m,
                                EpsGrowthYoYPercent = 17.9m
                            }
                        }
                    }
                }
            };
        }
    }
}

