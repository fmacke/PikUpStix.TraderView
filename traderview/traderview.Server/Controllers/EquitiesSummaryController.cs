using Microsoft.AspNetCore.Mvc;
using TraderView.Application.Interfaces.Services;
using traderview.Server.DTOs;

namespace traderview.Server.Controllers;

/// <summary>
/// Controller for equity summary endpoints
/// </summary>
[ApiController]
[Route("api/equities")]
public class EquitiesSummaryController : ControllerBase
{
    private readonly IEquitySummaryService _equitySummaryService;
    private readonly ILogger<EquitiesSummaryController> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    public EquitiesSummaryController(
        IEquitySummaryService equitySummaryService,
        ILogger<EquitiesSummaryController> logger)
    {
        _equitySummaryService = equitySummaryService;
        _logger = logger;
    }

    /// <summary>
    /// Get asset value over time for all accounts across a date range
    /// </summary>
    /// <param name="startDate">Start date for the range (inclusive)</param>
    /// <param name="endDate">End date for the range (inclusive)</param>
    /// <returns>List of asset values by date</returns>
    [HttpGet("asset-value-over-time")]
    [ProducesResponseType(typeof(List<AssetValueOverTimeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<AssetValueOverTimeDto>>> GetAssetValueOverTimeAsync(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
            {
                return BadRequest(new { message = "Start date must be before or equal to end date" });
            }

            var equitySummaries = await _equitySummaryService.GetByDateRangeAsync(startDate, endDate);

            // Group by date and aggregate total asset values
            var assetValuesByDate = equitySummaries
                .GroupBy(es => es.ReportDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new AssetValueOverTimeDto(
                    g.Key,
                    g.Sum(es => es.Total),
                    g.Sum(es => es.TotalLong),
                    g.Sum(es => Math.Abs(es.TotalShort))
                ))
                .ToList();

            return Ok(assetValuesByDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching asset value over time");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error fetching asset value over time", detail = ex.Message }
            );
        }
    }

    /// <summary>
    /// Get asset value over time for a specific account
    /// </summary>
    /// <param name="accountId">The account ID</param>
    /// <param name="startDate">Start date for the range (inclusive)</param>
    /// <param name="endDate">End date for the range (inclusive)</param>
    /// <returns>List of asset values by date for the account</returns>
    [HttpGet("asset-value-over-time/{accountId}")]
    [ProducesResponseType(typeof(List<AssetValueOverTimeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<AssetValueOverTimeDto>>> GetAssetValueOverTimeByAccountAsync(
        string accountId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
            {
                return BadRequest(new { message = "Start date must be before or equal to end date" });
            }

            if (string.IsNullOrWhiteSpace(accountId))
            {
                return BadRequest(new { message = "Account ID must not be empty" });
            }

            var equitySummaries = await _equitySummaryService.GetByAccountIdAsync(accountId);

            var filteredAndGrouped = equitySummaries
                .Where(es => es.ReportDate.Date >= startDate.Date && es.ReportDate.Date <= endDate.Date)
                .GroupBy(es => es.ReportDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new AssetValueOverTimeDto(
                    g.Key,
                    g.Sum(es => es.Total),
                    g.Sum(es => es.TotalLong),
                    g.Sum(es => Math.Abs(es.TotalShort))
                ))
                .ToList();

            return Ok(filteredAndGrouped);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching asset value over time for account {AccountId}", accountId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error fetching asset value over time", detail = ex.Message }
            );
        }
    }

    /// <summary>
    /// Get the latest equity summary across all accounts
    /// </summary>
    /// <returns>The most recent equity summary</returns>
    [HttpGet("latest")]
    [ProducesResponseType(typeof(AssetValueOverTimeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AssetValueOverTimeDto>> GetLatestAssetValueAsync()
    {
        try
        {
            var allSummaries = await _equitySummaryService.GetAllAsync();

            if (!allSummaries.Any())
            {
                return NotFound(new { message = "No equity summaries found" });
            }

            var latest = allSummaries
                .OrderByDescending(es => es.ReportDate)
                .GroupBy(es => es.ReportDate.Date)
                .FirstOrDefault();

            if (latest == null)
            {
                return NotFound(new { message = "No equity summaries found" });
            }

            var result = new AssetValueOverTimeDto(
                latest.Key,
                latest.Sum(es => es.Total),
                latest.Sum(es => es.TotalLong),
                latest.Sum(es => Math.Abs(es.TotalShort))
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching latest asset value");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error fetching latest asset value", detail = ex.Message }
            );
        }
    }
}
