using Microsoft.AspNetCore.Mvc;
using traderview.Server.DTOs;
using TraderView.Application.Interfaces.Services;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/aireports")]
    public class AIReportController : ControllerBase
    {
        private readonly IAIReportService _aiReportService;
        private readonly ILogger<AIReportController> _logger;

        public AIReportController(
            IAIReportService aiReportService,
            ILogger<AIReportController> logger)
        {
            _aiReportService = aiReportService;
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
    }
}
