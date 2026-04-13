using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBoardingHouse.Application.DTOs.Amenities;
using SpotBoardingHouse.Application.Interfaces.Services;

namespace SpotBoardingHouse.Api.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Owner,Admin")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class RoomAmenitiesController : ControllerBase
    {
        private readonly IRoomAmenityService _roomAmenityService;
        private readonly IRoomService _roomService;
        private readonly IBoardingHouseService _boardingHouseService;

        public RoomAmenitiesController(
            IRoomAmenityService roomAmenityService,
            IRoomService roomService,
            IBoardingHouseService boardingHouseService)
        {
            _roomAmenityService = roomAmenityService;
            _roomService = roomService;
            _boardingHouseService = boardingHouseService;
        }

        private bool TryGetCurrentUserId(out Guid userId)
        {
            userId = Guid.Empty;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out userId);
        }

        private async Task<(bool Exists, bool Authorized)> TryAuthorizeRoomAsync(Guid roomId)
        {
            var room = await _roomService.GetByIdAsync(roomId);
            if (room == null)
            {
                return (false, false);
            }

            if (User.IsInRole("Admin"))
            {
                return (true, true);
            }

            if (!TryGetCurrentUserId(out var userId))
            {
                return (true, false);
            }

            var boardingHouse = await _boardingHouseService.GetByIdAsync(room.BoardingHouseId);
            if (boardingHouse == null)
            {
                return (false, false);
            }

            return (true, boardingHouse.UserId == userId);
        }

        [HttpGet("room/{roomId:guid}")]
        public async Task<ActionResult<IEnumerable<AmenityDto>>> GetByRoom(Guid roomId)
        {
            var (exists, authorized) = await TryAuthorizeRoomAsync(roomId);

            if (!exists) return NotFound();
            if (!authorized) return Forbid();

            var amenities = await _roomAmenityService.GetAmenitiesByRoomAsync(roomId);

            return Ok(amenities);
        }

        [HttpPost("room/{roomId:guid}")]
        public async Task<IActionResult> Attach(Guid roomId, [FromBody] AttachAmenityDto attachAmenityDto)
        {
            try
            {
                var (exists, authorized) = await TryAuthorizeRoomAsync(roomId);

                if (!exists) return NotFound();
                if (!authorized) return Forbid();

                await _roomAmenityService.AttachAsync(roomId, attachAmenityDto.AmenityId);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("room/{roomId:guid}/amenity/{amenityId:guid}")]
        public async Task<IActionResult> Detach(Guid roomId, Guid amenityId)
        {
            var (exists, authorized) = await TryAuthorizeRoomAsync(roomId);

            if (!exists) return NotFound();
            if (!authorized) return Forbid();

            var detached = await _roomAmenityService.DetachAsync(roomId, amenityId);

            return detached ? NoContent() : NotFound();
        }
    }
}
