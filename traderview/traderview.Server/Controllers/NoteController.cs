using Microsoft.AspNetCore.Mvc;
using traderview.Server.DTOs;
using TraderView.Application.Interfaces.Services;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/notes")]
    public class NoteController : ControllerBase
    {
        private readonly INoteService _noteService;
        private readonly ILogger<NoteController> _logger;

        public NoteController(
            INoteService noteService,
            ILogger<NoteController> logger)
        {
            _noteService = noteService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new note
        /// </summary>
        /// <param name="createNoteDto">The note data to create</param>
        /// <returns>The created note with its ID</returns>
        [HttpPost]
        [ProducesResponseType(typeof(NoteDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NoteDto>> CreateNoteAsync([FromBody] CreateNoteDto createNoteDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createNoteDto.Comment))
                {
                    return BadRequest(new { message = "Comment is required" });
                }

                _logger.LogInformation("Creating note for position {PositionId}", createNoteDto.PositionId);

                var noteId = await _noteService.CreateAsync(
                    createNoteDto.PositionId,
                    createNoteDto.TradeExecutionId,
                    createNoteDto.Comment,
                    createNoteDto.EntryDate,
                    createNoteDto.TradeTypeId,
                    createNoteDto.ErrorTypeId
                );

                var createdNote = new NoteDto
                {
                    Id = noteId,
                    PositionId = createNoteDto.PositionId,
                    TradeExecutionId = createNoteDto.TradeExecutionId,
                    Comment = createNoteDto.Comment,
                    EntryDate = createNoteDto.EntryDate,
                    TradeTypeId = createNoteDto.TradeTypeId,
                    UpdatedAt = DateTime.UtcNow,
                    ErrorTypeId = createNoteDto.ErrorTypeId
                };

                _logger.LogInformation("Note created with ID {NoteId}", noteId);

                // Return 201 Created with the created note
                return StatusCode(StatusCodes.Status201Created, createdNote);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating note");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error creating note", detail = ex.Message }
                );
            }
        }

        /// <summary>
        /// Get a note by ID
        /// </summary>
        /// <param name="id">The note ID</param>
        /// <returns>The note if found</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(NoteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NoteDto>> GetNoteByIdAsync(int id)
        {
            try
            {
                var note = await _noteService.GetByIdAsync(id);

                if (note == null)
                {
                    return NotFound(new { message = $"Note with ID {id} not found" });
                }

                var noteDto = new NoteDto
                {
                    Id = note.Id,
                    PositionId = note.PositionId,
                    TradeExecutionId = note.TradeExecutionId,
                    Comment = note.Comment,
                    EntryDate = note.EntryDate,
                    TradeTypeId = note.TradeTypeId,
                    ErrorTypeId = note.ErrorTypeId,
                    UpdatedAt = note.UpdatedAt
                };

                return Ok(noteDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching note {NoteId}", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching note", detail = ex.Message }
                );
            }
        }

        /// <summary>
        /// Get all notes for a specific position
        /// </summary>
        /// <param name="positionId">The position ID</param>
        /// <returns>List of notes for the position</returns>
        [HttpGet("position/{positionId}")]
        [ProducesResponseType(typeof(List<NoteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<NoteDto>>> GetNotesByPositionIdAsync(int positionId)
        {
            try
            {
                var notes = await _noteService.GetByPositionIdAsync(positionId);

                var noteDtos = notes.Select(n => new NoteDto
                {
                    Id = n.Id,
                    PositionId = n.PositionId,
                    TradeExecutionId = n.TradeExecutionId,
                    Comment = n.Comment,
                    EntryDate = n.EntryDate,
                    UpdatedAt = n.UpdatedAt,
                    TradeTypeId = n.TradeTypeId,
                    ErrorTypeId = n.ErrorTypeId
                }).ToList();

                return Ok(noteDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notes for position {PositionId}", positionId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error fetching notes", detail = ex.Message }
                );
            }
        }

        /// <summary>
        /// Update an existing note
        /// </summary>
        /// <param name="updateNoteDto">The updated note data</param>
        /// <returns>The updated note</returns>
        [HttpPut]
        [ProducesResponseType(typeof(NoteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<NoteDto>> UpdateNoteAsync([FromBody] UpdateNoteDto updateNoteDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(updateNoteDto.Comment))
                {
                    return BadRequest(new { message = "Comment is required" });
                }

                _logger.LogInformation("Updating note with ID {NoteId}", updateNoteDto.Id);

                var existingNote = await _noteService.GetByIdAsync(updateNoteDto.Id);
                if (existingNote == null)
                {
                    return NotFound(new { message = $"Note with ID {updateNoteDto.Id} not found" });
                }

                var isUpdated = await _noteService.UpdateAsync(
                    updateNoteDto.Id,
                    existingNote.PositionId,
                    existingNote.TradeExecutionId,
                    updateNoteDto.Comment,
                    DateTime.Now,
                    updateNoteDto.TradeTypeId,
                    updateNoteDto.ErrorTypeId
                );

                if (!isUpdated)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new { message = "Failed to update note" }
                    );
                }

                var updatedNote = new NoteDto
                {
                    Id = updateNoteDto.Id,
                    PositionId = existingNote.PositionId,
                    TradeExecutionId = existingNote.TradeExecutionId,
                    Comment = updateNoteDto.Comment,
                    EntryDate = updateNoteDto.EntryDate,
                    TradeTypeId = updateNoteDto.TradeTypeId,
                    UpdatedAt = DateTime.UtcNow,
                    ErrorTypeId = updateNoteDto.ErrorTypeId
                };

                _logger.LogInformation("Note with ID {NoteId} updated successfully", updateNoteDto.Id);

                return Ok(updatedNote);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating note");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Error updating note", detail = ex.Message }
                );
            }
        }
    }
}
