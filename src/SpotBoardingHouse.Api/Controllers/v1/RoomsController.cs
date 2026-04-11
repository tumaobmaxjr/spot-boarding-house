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

        private bool TryGetCurrentUserId(out Guid userId)
        {
            userId = Guid.Empty;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out userId);
        }

        private async Task<(RoomDto? Room, bool Exists, bool Authorized)> TryGetAuthorizedRoomAsync(Guid roomId)
        {
            var room = await _roomService.GetByIdAsync(roomId);
            if (room == null)
            {
                return (null, false, false);
            }

            if (User.IsInRole("Admin"))
            {
                return (room, true, true);
            }

            if (!TryGetCurrentUserId(out var userId))
            {
                return (room, true, false);
            }

            var boardingHouse = await _boardingHouseService.GetByIdAsync(room.BoardingHouseId);
            if (boardingHouse == null)
            {
                return (null, false, false); // treat as not found
            }

            return (room, true, boardingHouse.UserId == userId);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetAll()
        {
            var isAdmin = User.IsInRole("Admin");

            if (isAdmin)
            {
                var allRooms = await _roomService.GetAllAsync();
                return Ok(allRooms);
            }

            if (!TryGetCurrentUserId(out var userId))
            {
                return Forbid();
            }

            var ownerRooms = await _roomService.GetAllByUserIdAsync(userId);

            return Ok(ownerRooms);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<RoomDto>> GetById(Guid id)
        {
            var (room, exists, authorized) = await TryGetAuthorizedRoomAsync(id);

            if (!exists) return NotFound();
            if (!authorized) return Forbid();
            if (room == null) return NotFound();

            return Ok(room);
        }

        [HttpPost]
        public async Task<ActionResult<RoomDto>> Create([FromBody] CreateRoomDto createRoomDto)
        {
            try
            {
                var isAdmin = User.IsInRole("Admin");

                var boardingHouse = await _boardingHouseService.GetByIdAsync(createRoomDto.BoardingHouseId);
                if (boardingHouse == null)
                {
                    return NotFound();
                }

                if (!isAdmin)
                {
                    if (!TryGetCurrentUserId(out var userId))
                    {
                        return Forbid();
                    }

                    if (boardingHouse.UserId != userId)
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
                var (existing, exists, authorized) = await TryGetAuthorizedRoomAsync(id);

                if (!exists) return NotFound();
                if (!authorized) return Forbid();
                if (existing == null) return NotFound();

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
            var (existing, exists, authorized) = await TryGetAuthorizedRoomAsync(id);

            if (!exists) return NotFound();
            if (!authorized) return Forbid();
            if (existing == null) return NotFound();

            var deleted = await _roomService.DeleteAsync(id, existing.BoardingHouseId);

            return deleted ? NoContent() : NotFound();
        }
    }
}
