using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using traderview.Server.DTOs;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly ILogger<ReportsController> _logger;
        private readonly IOpenPositionReportService _openPositionReportService;
        private readonly IPositionService _positionService;
        private readonly ITradeHistoryReportService _tradeHistoryReportService;
        private readonly ICompanyReportService _companyReportService;
        private readonly ICompanyChecklistReportQueryService _companyChecklistReportQueryService;
        private readonly ICompanyChecklistReportExcelService _companyChecklistReportExcelService;
        private readonly IMapper _mapper;

        public ReportsController(
            ILogger<ReportsController> logger,
            IOpenPositionReportService openPositionsReportService,
            IPositionService positionService,
            ITradeHistoryReportService tradeHistoryReportService,
            ICompanyReportService companyReportService,
            ICompanyChecklistReportQueryService companyChecklistReportQueryService,
            ICompanyChecklistReportExcelService companyChecklistReportExcelService,
            IMapper mapper)
        {
            _logger = logger;
            _openPositionReportService = openPositionsReportService;
            _tradeHistoryReportService = tradeHistoryReportService;
            _positionService = positionService;
            _companyReportService = companyReportService;
            _companyChecklistReportQueryService = companyChecklistReportQueryService;
            _companyChecklistReportExcelService = companyChecklistReportExcelService;
            _mapper = mapper;
        }

        [HttpGet("position-outcomes")]
        [ProducesResponseType(typeof(List<PositionOutcomeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<PositionOutcomeDto>>> GetPositionOutcomes()
        {
            try
            {
                _logger.LogInformation("GetPositionOutcomes called");
                var positions = await _positionService.GetClosedPositionsAsync();
                var outcomes = await _tradeHistoryReportService.CreateTradeHistoryReport(positions);
                _logger.LogInformation("CalculatePositionOutcomesAsync returned {Count} items", outcomes?.Count ?? 0);

                var dtos = outcomes.Select(o => new PositionOutcomeDto
                {
                    Id = o.Id,
                    OpenDate = o.OpenDate,
                    CloseDate = o.CloseDate,
                    Status = null,
                    InstrumentId = o.InstrumentId,
                    SymbolName = o.Symbol,
                    LastReportedPrice = null,
                    LastReportedPriceUpdated = null,
                    MaximumFavourableExcursion = o.MaximumFavourableExcursion,
                    MaximumAdverseExcursion = o.MaximumAdverseExcursion,
                    Margin = o.NetMargin,
                    MarketRegime = o.MarketRegime
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching position outcomes report");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error generating report", detail = ex.Message });
            }
        }
        [HttpGet("position-outcome")]
        [ProducesResponseType(typeof(List<PositionOutcomeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<PositionOutcomeDto>>> GetPositionOutcome(int id)
        {
            try
            {
                _logger.LogInformation("GetPositionOutcome called with id={Id}", id);
                var position = await _positionService.GetPositionByIdAsync(id);
                if (position == null)
                {
                    return NotFound();
                }
                var outcome = await _openPositionReportService.GetPositionReport(position);
                if (outcome == null)
                {
                    return NotFound();
                }

                var dto = new PositionOutcomeDto
                {
                    Id = outcome.Id,
                    OpenDate = outcome.OpenDate,
                    CloseDate = outcome.CloseDate,
                    Status = outcome.Status,
                    InstrumentId = outcome.InstrumentId,
                    SymbolName = outcome.SymbolName,
                    LastReportedPrice = outcome.LastReportedPrice,
                    LastReportedPriceUpdated = outcome.LastReportedPriceUpdated,
                    MarketRegime = outcome.MarketRegime,
                    MaximumFavourableExcursion = outcome.MaximumFavourableExcursion,
                    MaximumAdverseExcursion = outcome.MaximumAdverseExcursion,
                    Margin = outcome.Margin
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching position outcomes report");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error generating report", detail = ex.Message });
            }
        }

        [HttpGet("company-checklist/{symbol}")]
        [ProducesResponseType(typeof(CompanyChecklistReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CompanyChecklistReportDto>> GetCompanyChecklistReport(string symbol)
        {
            try
            {
                _logger.LogInformation("GetCompanyChecklistReport called for symbol={Symbol}", symbol);
                var report = await _companyReportService.GenerateChecklistReportAsync(symbol);

                if (report == null)
                {
                    return NotFound(new { message = $"No checklist report found for symbol {symbol}" });
                }

                // Map from Application model to Domain entity and save to database
                var entity = new CompanyChecklistReport
                {
                    Symbol = report.Symbol,
                    CompanyName = report.CompanyName,
                    CreatedAtUtc = DateTime.UtcNow,
                    CompositeRating = report.CompositeRating,
                    MarketExposureStatus = report.MarketExposureStatus,
                    IndustryGroup = report.IndustryGroup,
                    IndustryGroupRank = report.IndustryGroupRank,
                    GroupRsRating = report.GroupRsRating,
                    EpsDueDate = report.EpsDueDate,
                    EpsRating = report.EpsRating,
                    LastQtrEpsGrowthYoY = report.LastQtrEpsGrowthYoY,
                    Last3QtrsAvgEpsGrowth = report.Last3QtrsAvgEpsGrowth,
                    QtrsEpsAccelerationCount = report.QtrsEpsAccelerationCount,
                    CurrentQtrEpsEstChg = report.CurrentQtrEpsEstChg,
                    EstimateRevisionsTrend = report.EstimateRevisionsTrend,
                    LastQtrEarningsSurprisePercent = report.LastQtrEarningsSurprisePercent,
                    Eps3YrCagr = report.Eps3YrCagr,
                    ConsecutiveYrsAnnualEpsGrowth = report.ConsecutiveYrsAnnualEpsGrowth,
                    CurrentYearEpsEstChg = report.CurrentYearEpsEstChg,
                    SmrRating = report.SmrRating,
                    LastQtrSalesGrowthYoY = report.LastQtrSalesGrowthYoY,
                    Sales3YrCagr = report.Sales3YrCagr,
                    AnnualPreTaxMarginPercent = report.AnnualPreTaxMarginPercent,
                    AnnualRoePercent = report.AnnualRoePercent,
                    DebtToEquityRatio = report.DebtToEquityRatio,
                    Price = report.Price,
                    RelativeStrengthRating = report.RelativeStrengthRating,
                    PercentOff52WeekHigh = report.PercentOff52WeekHigh,
                    PriceVs50DaySmaPercent = report.PriceVs50DaySmaPercent,
                    Volume50DayAverage = report.Volume50DayAverage,
                    MarketCapBillions = report.MarketCapBillions,
                    AccumulationDistributionRating = report.AccumulationDistributionRating,
                    UpDownVolumeRatio = report.UpDownVolumeRatio,
                    FundOwnershipChangePercent = report.FundOwnershipChangePercent
                };

                // Save the report to database
                var reportId = await _companyReportService.SaveChecklistReportAsync(entity);
                _logger.LogInformation("Successfully saved company checklist report with ID={ReportId} for symbol={Symbol}", reportId, symbol);

                var dto = _mapper.Map<CompanyChecklistReportDto>(report);
                _logger.LogInformation("Successfully generated company checklist report for {Symbol}", symbol);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating company checklist report for symbol={Symbol}", symbol);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error generating company checklist report", detail = ex.Message });
            }
        }

        [HttpPost("company-checklists-export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        public async Task<IActionResult> ExportChecklistReportsToExcel([FromBody] ExportChecklistReportsRequest request)
        {
            try
            {
                if (request?.Symbols == null || request.Symbols.Count == 0)
                {
                    return BadRequest(new { message = "Symbols list cannot be null or empty." });
                }

                _logger.LogInformation("ExportChecklistReportsToExcel called with {SymbolCount} symbols", request.Symbols.Count);

                // Generate fresh reports from FMP and save to database
                var reports = new List<CompanyChecklistReport>();
                var successCount = 0;
                var failureCount = 0;

                foreach (var symbol in request.Symbols)
                {
                    if (string.IsNullOrWhiteSpace(symbol))
                    {
                        _logger.LogWarning("Skipping empty symbol in export request");
                        failureCount++;
                        continue;
                    }

                    try
                    {
                        // Generate fresh report from FMP
                        _logger.LogInformation("Generating fresh checklist report from FMP for symbol={Symbol}", symbol);
                        var freshReport = await _companyReportService.GenerateChecklistReportAsync(symbol);

                        if (freshReport == null)
                        {
                            _logger.LogWarning("No checklist report generated for symbol {Symbol} from FMP", symbol);
                            failureCount++;
                            continue;
                        }

                        // Map from Application model to Domain entity and save to database
                        var entity = new CompanyChecklistReport
                        {
                            Symbol = freshReport.Symbol,
                            CompanyName = freshReport.CompanyName,
                            CreatedAtUtc = DateTime.UtcNow,
                            CompositeRating = freshReport.CompositeRating,
                            MarketExposureStatus = freshReport.MarketExposureStatus,
                            IndustryGroup = freshReport.IndustryGroup,
                            IndustryGroupRank = freshReport.IndustryGroupRank,
                            GroupRsRating = freshReport.GroupRsRating,
                            EpsDueDate = freshReport.EpsDueDate,
                            EpsRating = freshReport.EpsRating,
                            LastQtrEpsGrowthYoY = freshReport.LastQtrEpsGrowthYoY,
                            Last3QtrsAvgEpsGrowth = freshReport.Last3QtrsAvgEpsGrowth,
                            QtrsEpsAccelerationCount = freshReport.QtrsEpsAccelerationCount,
                            CurrentQtrEpsEstChg = freshReport.CurrentQtrEpsEstChg,
                            EstimateRevisionsTrend = freshReport.EstimateRevisionsTrend,
                            LastQtrEarningsSurprisePercent = freshReport.LastQtrEarningsSurprisePercent,
                            Eps3YrCagr = freshReport.Eps3YrCagr,
                            ConsecutiveYrsAnnualEpsGrowth = freshReport.ConsecutiveYrsAnnualEpsGrowth,
                            CurrentYearEpsEstChg = freshReport.CurrentYearEpsEstChg,
                            SmrRating = freshReport.SmrRating,
                            LastQtrSalesGrowthYoY = freshReport.LastQtrSalesGrowthYoY,
                            Sales3YrCagr = freshReport.Sales3YrCagr,
                            AnnualPreTaxMarginPercent = freshReport.AnnualPreTaxMarginPercent,
                            AnnualRoePercent = freshReport.AnnualRoePercent,
                            DebtToEquityRatio = freshReport.DebtToEquityRatio,
                            Price = freshReport.Price,
                            RelativeStrengthRating = freshReport.RelativeStrengthRating,
                            PercentOff52WeekHigh = freshReport.PercentOff52WeekHigh,
                            PriceVs50DaySmaPercent = freshReport.PriceVs50DaySmaPercent,
                            Volume50DayAverage = freshReport.Volume50DayAverage,
                            MarketCapBillions = freshReport.MarketCapBillions,
                            AccumulationDistributionRating = freshReport.AccumulationDistributionRating,
                            UpDownVolumeRatio = freshReport.UpDownVolumeRatio,
                            FundOwnershipChangePercent = freshReport.FundOwnershipChangePercent
                        };

                        // Save the report to database
                        var reportId = await _companyReportService.SaveChecklistReportAsync(entity);
                        _logger.LogInformation("Successfully saved company checklist report with ID={ReportId} for symbol={Symbol}", reportId, symbol);

                        // Add domain entity to reports list for Excel export
                        reports.Add(entity);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error generating or saving checklist report for symbol={Symbol}", symbol);
                        failureCount++;
                        // Continue processing other symbols even if one fails
                    }
                }

                if (reports.Count == 0)
                {
                    return BadRequest(new { message = "No checklist reports could be generated for any of the provided symbols.", successCount, failureCount });
                }

                _logger.LogInformation("Generated and saved {SuccessCount} reports (failures: {FailureCount}) for bulk export", successCount, failureCount);

                // Sort reports by symbol for consistent output
                var sortedReports = reports.OrderBy(r => r.Symbol).ToList();

                // Generate Excel file
                var excelContent = await _companyChecklistReportExcelService.GenerateChecklistReportsExcelAsync(sortedReports);

                // Return file for download
                var fileName = $"CompanyChecklists_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
                _logger.LogInformation("Successfully generated Excel export with {ReportCount} reports: {FileName}", sortedReports.Count, fileName);

                return File(excelContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid argument for checklist export");
                return BadRequest(new { message = "Invalid request parameters", detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting checklist reports to Excel");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error generating Excel export", detail = ex.Message });
            }
        }
    }
}

