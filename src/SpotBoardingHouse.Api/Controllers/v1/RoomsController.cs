using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBoardingHouse.Application.DTOs.Rooms;
using SpotBoardingHouse.Application.Interfaces.Services;

namespace SpotBoardingHouse.Api.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Owner,Admin")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly IBoardingHouseService _boardingHouseService;

        public RoomsController(IRoomService roomService, IBoardingHouseService boardingHouseService)
        {
            _roomService = roomService;
            _boardingHouseService = boardingHouseService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userIdClaim!);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetAll()
        {
            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            var rooms = isAdmin ? await _roomService.GetAllAsync() : await _roomService.GetAllByUserIdAsync(userId);

            return Ok(rooms);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<RoomDto>> GetById(Guid id)
        {
            var room = await _roomService.GetByIdAsync(id);
            return room == null ? NotFound() : Ok(room);
        }

        [HttpPost]
        public async Task<ActionResult<RoomDto>> Create([FromBody] CreateRoomDto createRoomDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isAdmin = User.IsInRole("Admin");

                if (!isAdmin)
                {
                    var bh = await _boardingHouseService.GetByIdAsync(createRoomDto.BoardingHouseId);
                    if (bh == null || bh.UserId != userId)
                    {
                        return Forbid();
                    }
                }

                var room = await _roomService.CreateAsync(createRoomDto.BoardingHouseId, createRoomDto);

                return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<RoomDto>> Update(Guid id, [FromBody] UpdateRoomDto updateRoomDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isAdmin = User.IsInRole("Admin");

                var existing = await _roomService.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound();
                }

                if (!isAdmin)
                {
                    var bh = await _boardingHouseService.GetByIdAsync(existing.BoardingHouseId);
                    if (bh == null || bh.UserId != userId)
                    {
                        return Forbid();
                    }
                }

                var updated = await _roomService.UpdateAsync(id, existing.BoardingHouseId, updateRoomDto);

                return updated == null ? NotFound() : Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            var existing = await _roomService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (!isAdmin)
            {
                var bh = await _boardingHouseService.GetByIdAsync(existing.BoardingHouseId);
                if (bh == null || bh.UserId != userId)
                {
                    return Forbid();
                }
            }

            var deleted = await _roomService.DeleteAsync(id, existing.BoardingHouseId);

            return deleted ? NoContent() : NotFound();
        }
    }
}
