using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TraderView.Application.Interfaces.Services;
using traderview.Server.DTOs;
using TraderView.Domain.Entities;

namespace traderview.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PositionCalculatorController : ControllerBase
    {
        private readonly IPositionCalculatorService _service;
        private readonly ILogger<PositionCalculatorController> _logger;
        private readonly IMapper _mapper;

        public PositionCalculatorController(ILogger<PositionCalculatorController> logger, IPositionCalculatorService service, IMapper mapper)
        {
            _logger = logger;
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PositionCalculatorDto>>> GetAll()
        {
            var items = await _service.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<PositionCalculatorDto>>(items);
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PositionCalculatorDto>> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null)
                return NotFound();
            var dto = _mapper.Map<PositionCalculatorDto>(item);
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<PositionCalculatorDto>> Create([FromBody] PositionCalculatorCreateDto request)
        {
            if (request == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entity = _mapper.Map<PositionCalculator>(request);
            var created = await _service.CreateAsync(entity);
            var dto = _mapper.Map<PositionCalculatorDto>(created);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PositionCalculatorUpdateDto request)
        {
            if (request == null || id != request.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            var entity = _mapper.Map<PositionCalculator>(request);
            await _service.UpdateAsync(entity);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
