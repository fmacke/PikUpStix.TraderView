using Microsoft.AspNetCore.Mvc;
using traderview.Server.DTOs;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Interfaces.Services.AI;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/aireports")]
    public class AIReportController : ControllerBase
    {
        private readonly IAIReportService _aiReportService;
        private readonly IGoogleAIStudioService _googleAIStudioService;
        private readonly IMarketDataService _marketDataService;
        private readonly ILogger<AIReportController> _logger;

        public AIReportController(
            IAIReportService aiReportService,
            IGoogleAIStudioService googleAIStudioService,
            IMarketDataService marketDataService,
            ILogger<AIReportController> logger)
        {
            _aiReportService = aiReportService;
            _googleAIStudioService = googleAIStudioService;
            _marketDataService = marketDataService;
            _logger = logger;
        }

        /// <summary>
        /// Get all AI reports
        /// </summary>
        /// <returns>A list of all AI reports sorted by creation date</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<AIReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<AIReportDto>>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all AI reports");

                var reports = await _aiReportService.GetAllAsync();

                var reportDtos = reports.Select(report => new AIReportDto
                {
                    Id = report.Id,
                    ReportInstructionId = report.ReportInstructionId,
                    ReportDetail = report.ReportDetail,
                    CreatedAt = report.CreatedAt
                }).ToList();

                return Ok(reportDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching AI reports");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching AI reports", detail = ex.Message }
                );
            }
        }

        /// <summary>
        /// Get an AI report by ID
        /// </summary>
        /// <param name="id">The report ID</param>
        /// <returns>The AI report if found</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AIReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AIReportDto>> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching AI report with ID {ReportId}", id);

                var report = await _aiReportService.GetByIdAsync(id);

                if (report == null)
                {
                    return NotFound(new { message = $"AI report with ID {id} not found" });
                }

                var reportDto = new AIReportDto
                {
                    Id = report.Id,
                    ReportInstructionId = report.ReportInstructionId,
                    ReportDetail = report.ReportDetail,
                    CreatedAt = report.CreatedAt
                };

                return Ok(reportDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching AI report with ID {ReportId}", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching AI report", detail = ex.Message }
                );
            }
        }

        /// <summary>
        /// Create a new AI report
        /// </summary>
        /// <param name="createReportDto">The report data to create</param>
        /// <returns>The created report with its ID</returns>
        [HttpPost]
        [ProducesResponseType(typeof(AIReportDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AIReportDto>> CreateAsync([FromBody] CreateAIReportDto createReportDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createReportDto.ReportDetail))
                {
                    return BadRequest(new { message = "Report detail is required" });
                }

                _logger.LogInformation("Creating AI report with instruction ID {InstructionId}", createReportDto.ReportInstructionId);

                var reportId = await _aiReportService.CreateAsync(
                    createReportDto.ReportInstructionId,
                    createReportDto.ReportDetail
                );

                var createdReport = new AIReportDto
                {
                    Id = reportId,
                    ReportInstructionId = createReportDto.ReportInstructionId,
                    ReportDetail = createReportDto.ReportDetail,
                    CreatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("AI report created with ID {ReportId}", reportId);

                return StatusCode(StatusCodes.Status201Created, createdReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating AI report");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error creating AI report", detail = ex.Message }
                );
            }
        }

        /// <summary>
        /// Update an existing AI report
        /// </summary>
        /// <param name="id">The report ID</param>
        /// <param name="updateReportDto">The updated report data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateAIReportDto updateReportDto)
        {
            try
            {
                if (id != updateReportDto.Id)
                {
                    return BadRequest(new { message = "ID mismatch" });
                }

                if (string.IsNullOrWhiteSpace(updateReportDto.ReportDetail))
                {
                    return BadRequest(new { message = "Report detail is required" });
                }

                _logger.LogInformation("Updating AI report with ID {ReportId}", id);

                var updated = await _aiReportService.UpdateAsync(
                    id,
                    updateReportDto.ReportInstructionId,
                    updateReportDto.ReportDetail
                );

                if (!updated)
                {
                    return NotFound(new { message = $"AI report with ID {id} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating AI report with ID {ReportId}", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error updating AI report", detail = ex.Message }
                );
            }
        }

        /// <summary>
        /// Delete an AI report by ID
        /// </summary>
        /// <param name="id">The report ID</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting AI report with ID {ReportId}", id);

                var deleted = await _aiReportService.DeleteAsync(id);

                if (!deleted)
                {
                    return NotFound(new { message = $"AI report with ID {id} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting AI report with ID {ReportId}", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error deleting AI report", detail = ex.Message }
                );
            }
        }

        /// <summary>
        /// Generate an AI report based on instruction and open positions
        /// </summary>
        /// <param name="generateReportDto">The instruction ID and positions for report generation</param>
        /// <returns>The generated report text</returns>
        [HttpPost("generate")]
        [ProducesResponseType(typeof(GenerateAIReportResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GenerateAIReportResponseDto>> GenerateReportAsync([FromBody] GenerateAIReportDto generateReportDto)
        {
            try
            {
                if (generateReportDto.InstructionId <= 0)
                {
                    return BadRequest(new { message = "Valid instruction ID is required" });
                }

                _logger.LogInformation("Generating AI report with instruction ID {InstructionId}", generateReportDto.InstructionId);

                var report = await _googleAIStudioService.GenerateReportAsync(
                    generateReportDto.InstructionId,
                    generateReportDto.OpenPositions ?? new List<TraderView.Domain.Entities.Position>()
                );

                _logger.LogInformation("AI report generated successfully for instruction ID {InstructionId}", generateReportDto.InstructionId);

                return Ok(new GenerateAIReportResponseDto
                {
                    Report = report,
                    GeneratedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI report with instruction ID {InstructionId}", generateReportDto.InstructionId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error generating AI report", detail = ex.Message }
                );
            }
        }

        /// <summary>
        /// Fetch market quotes for given tickers
        /// </summary>
        /// <param name="tickers">Comma-separated list of tickers (e.g., "AAPL,MSFT,^GSPC")</param>
        /// <returns>JSON market quote data</returns>
        [HttpGet("market-data/quotes")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> GetMarketQuotesAsync([FromQuery] string tickers)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tickers))
                {
                    return BadRequest(new { message = "Tickers parameter is required" });
                }

                _logger.LogInformation("Fetching market quotes for tickers: {Tickers}", tickers);

                var quotesJson = await _marketDataService.GetMarketQuotesAsync(tickers);
                var quotes = System.Text.Json.JsonSerializer.Deserialize<object>(quotesJson);

                _logger.LogInformation("Successfully retrieved market quotes for {Tickers}", tickers);

                return Ok(quotes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching market quotes for tickers: {Tickers}", tickers);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching market quotes", detail = ex.Message }
                );
            }
        }

        /// <summary>
        /// Fetch stock news for given tickers
        /// </summary>
        /// <param name="tickers">Comma-separated list of tickers</param>
        /// <param name="limit">Maximum number of news articles to retrieve (default: 5)</param>
        /// <returns>JSON stock news data</returns>
        [HttpGet("market-data/news")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> GetStockNewsAsync([FromQuery] string tickers, [FromQuery] int limit = 5)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tickers))
                {
                    return BadRequest(new { message = "Tickers parameter is required" });
                }

                if (limit < 1)
                {
                    return BadRequest(new { message = "Limit must be greater than 0" });
                }

                _logger.LogInformation("Fetching stock news for tickers: {Tickers} with limit: {Limit}", tickers, limit);

                var newsJson = await _marketDataService.GetStockNewsAsync(tickers, limit);
                var news = System.Text.Json.JsonSerializer.Deserialize<object>(newsJson);

                _logger.LogInformation("Successfully retrieved stock news for {Tickers}", tickers);

                return Ok(news);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stock news for tickers: {Tickers}", tickers);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching stock news", detail = ex.Message }
                );
            }
        }

        /// <summary>
        /// Fetch economic calendar data
        /// </summary>
        /// <returns>JSON economic calendar data</returns>
        [HttpGet("market-data/economic-calendar")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> GetEconomicCalendarAsync()
        {
            try
            {
                _logger.LogInformation("Fetching economic calendar data");

                var calendarJson = await _marketDataService.GetEconomicCalendarAsync();
                var calendar = System.Text.Json.JsonSerializer.Deserialize<object>(calendarJson);

                _logger.LogInformation("Successfully retrieved economic calendar data");

                return Ok(calendar);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching economic calendar data");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching economic calendar", detail = ex.Message }
                );
            }
        }
    }
}
