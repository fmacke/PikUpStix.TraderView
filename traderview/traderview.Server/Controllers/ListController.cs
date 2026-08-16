using Microsoft.AspNetCore.Mvc;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/lists")]
    public class ListController : ControllerBase
    {
        private readonly IListService _listService;
        private readonly ILogger<ListController> _logger;

        public ListController(
            IListService listService,
            ILogger<ListController> logger)
        {
            _listService = listService;
            _logger = logger;
        }

        /// <summary>
        /// Get all list items for a specific list name (category)
        /// </summary>
        /// <param name="listName">The list name/category to filter by</param>
        /// <returns>List of items matching the list name</returns>
        [HttpGet("{listName}")]
        [ProducesResponseType(typeof(List<ListItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ListItem>>> GetByListNameAsync(string listName)
        {
            try
            {
                _logger.LogInformation("Fetching list items for list name: {Category}", listName);
                var items = await _listService.GetByListNameAsync(listName);
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching list items for list name {Category}", listName);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching list items", detail = ex.Message }
                );
            }
        }

        /// <summary>
        /// Get all EntryMethod items (convenience endpoint)
        /// </summary>
        /// <returns>List of EntryMethod items</returns>
        [HttpGet("entrymethod")]
        [ProducesResponseType(typeof(List<ListItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ListItem>>> GetEntryMethodsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching EntryMethod list items");
                var items = await _listService.GetByListNameAsync("EntryMethod");
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching EntryMethod list items");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching EntryMethod items", detail = ex.Message }
                );
            }
        }

        /// <summary>
        /// Get all distinct list names
        /// </summary>
        /// <returns>List of unique list names</returns>
        [HttpGet("names")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<string>>> GetDistinctListNamesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching distinct list names");
                var listNames = await _listService.GetDistinctListNamesAsync();
                return Ok(listNames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching distinct list names");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching list names", detail = ex.Message }
                );
            }
        }
    }
}
