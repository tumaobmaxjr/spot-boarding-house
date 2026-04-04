using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBoardingHouse.Application.DTOs.RoomTypes;
using SpotBoardingHouse.Application.Interfaces.Services;

namespace SpotBoardingHouse.Api.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class RoomTypesController : ControllerBase
    {
        private readonly IRoomTypeService _roomTypeService;

        public RoomTypesController(IRoomTypeService roomTypeService)
        {
            _roomTypeService = roomTypeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomTypeDto>>> GetAll()
        {
            var items = await _roomTypeService.GetAllAsync();

            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<RoomTypeDto>> GetById(Guid id)
        {
            var item = await _roomTypeService.GetByIdAsync(id);

            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<RoomTypeDto>> Create([FromBody] CreateRoomTypeDto createRoomTypeDto)
        {
            try
            {
                var item = await _roomTypeService.CreateAsync(createRoomTypeDto);

                return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<RoomTypeDto>> Update(Guid id, [FromBody] UpdateRoomTypeDto updateRoomTypeDto)
        {
            try
            {
                var item = await _roomTypeService.UpdateAsync(id, updateRoomTypeDto);

                return item == null ? NotFound() : Ok(item);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _roomTypeService.DeleteAsync(id);

            return deleted ? NoContent() : NotFound();
        }
    }
}
