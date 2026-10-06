using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TraderView.Application.Interfaces.Services;
using traderview.Server.DTOs;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/positions/{positionId:int}/calculators")]
    public class PositionCalculatorLinkController : ControllerBase
    {
        private readonly IPositionCalculatorService _service;
        private readonly IMapper _mapper;

        public PositionCalculatorLinkController(IPositionCalculatorService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetForPosition(int positionId)
        {
            var items = await _service.GetByPositionIdAsync(positionId);
            var dtos = _mapper.Map<IEnumerable<PositionCalculatorDto>>(items);
            return Ok(dtos);
        }

        [HttpGet("candidates")]
        public async Task<IActionResult> GetCandidates(int positionId)
        {
            var items = await _service.GetCandidatesForPositionAsync(positionId);
            var dtos = _mapper.Map<IEnumerable<PositionCalculatorDto>>(items);
            return Ok(dtos);
        }

        public class LinkRequest { public int[] CalculatorIds { get; set; } = Array.Empty<int>(); }

        [HttpPost("link")]
        public async Task<IActionResult> Link(int positionId, [FromBody] LinkRequest request)
        {
            if (request == null || request.CalculatorIds == null || request.CalculatorIds.Length == 0)
                return BadRequest("No calculator ids provided.");

            await _service.LinkCalculatorsAsync(positionId, request.CalculatorIds);
            return NoContent();
        }

        [HttpPost("unlink")]
        public async Task<IActionResult> Unlink(int positionId, [FromBody] LinkRequest request)
        {
            if (request == null || request.CalculatorIds == null || request.CalculatorIds.Length == 0)
                return BadRequest("No calculator ids provided.");

            await _service.UnlinkCalculatorsAsync(request.CalculatorIds);
            return NoContent();
        }
    }
}
