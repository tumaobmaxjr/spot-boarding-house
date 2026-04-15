using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBoardingHouse.Application.DTOs.Media;
using SpotBoardingHouse.Application.Interfaces.Services;

namespace SpotBoardingHouse.Api.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Owner,Admin")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly IMediaService _mediaService;
        private readonly IBoardingHouseService _boardingHouseService;
        private readonly IRoomService _roomService;

        public MediaController(
            IMediaService mediaService,
            IBoardingHouseService boardingHouseService,
            IRoomService roomService)
        {
            _mediaService = mediaService;
            _boardingHouseService = boardingHouseService;
            _roomService = roomService;
        }

        private bool TryGetCurrentUserId(out Guid userId)
        {
            userId = Guid.Empty;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out userId);
        }

        private async Task<(bool Exists, bool Authorized)> TryAuthorizeBoardingHouseAsync(Guid boardingHouseId)
        {
            var boardingHouse = await _boardingHouseService.GetByIdAsync(boardingHouseId);
            if (boardingHouse == null)
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

            return (true, boardingHouse.UserId == userId);
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

        private async Task<(MediaDto? Media, bool Exists, bool Authorized)> TryAuthorizeMediaAsync(Guid id)
        {
            var media = await _mediaService.GetByIdAsync(id);
            if (media == null)
            {
                return (null, false, false);
            }

            if (media.BoardingHouseId.HasValue)
            {
                var (exists, authorized) = await TryAuthorizeBoardingHouseAsync(media.BoardingHouseId.Value);
                if (!exists) return (null, false, false);
                if (!authorized) return (media, true, false);

                return (media, true, true);
            }

            if (media.RoomId.HasValue)
            {
                var (exists, authorized) = await TryAuthorizeRoomAsync(media.RoomId.Value);
                if (!exists) return (null, false, false);
                if (!authorized) return (media, true, false);

                return (media, true, true);
            }

            // Defensive fallback (should not happen if XOR constraints are enforced)
            return (null, false, false);
        }

        [HttpGet("boarding-house/{boardingHouseId:guid}")]
        public async Task<ActionResult<IEnumerable<MediaDto>>> GetByBoardingHouse(Guid boardingHouseId)
        {
            var (exists, authorized) = await TryAuthorizeBoardingHouseAsync(boardingHouseId);
            if (!exists) return NotFound();
            if (!authorized) return Forbid();

            var media = await _mediaService.GetAllByBoardingHouseIdAsync(boardingHouseId);

            return Ok(media);
        }

        [HttpGet("room/{roomId:guid}")]
        public async Task<ActionResult<IEnumerable<MediaDto>>> GetByRoom(Guid roomId)
        {
            var (exists, authorized) = await TryAuthorizeRoomAsync(roomId);
            if (!exists) return NotFound();
            if (!authorized) return Forbid();

            var media = await _mediaService.GetAllByRoomIdAsync(roomId);

            return Ok(media);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<MediaDto>> GetById(Guid id)
        {
            var (media, exists, authorized) = await TryAuthorizeMediaAsync(id);
            if (!exists) return NotFound();
            if (!authorized) return Forbid();
            if (media == null) return NotFound();

            return Ok(media);
        }

        [HttpPost]
        public async Task<ActionResult<MediaDto>> Create([FromBody] CreateMediaDto createMediaDto)
        {
            try
            {
                if (createMediaDto.BoardingHouseId.HasValue)
                {
                    var (exists, authorized) = await TryAuthorizeBoardingHouseAsync(createMediaDto.BoardingHouseId.Value);
                    if (!exists) return NotFound();
                    if (!authorized) return Forbid();
                }
                else if (createMediaDto.RoomId.HasValue)
                {
                    var (exists, authorized) = await TryAuthorizeRoomAsync(createMediaDto.RoomId.Value);
                    if (!exists) return NotFound();
                    if (!authorized) return Forbid();
                }

                var created = await _mediaService.CreateAsync(createMediaDto);

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<MediaDto>> Update(Guid id, [FromBody] UpdateMediaDto updateMediaDto)
        {
            try
            {
                var (existing, exists, authorized) = await TryAuthorizeMediaAsync(id);
                if (!exists) return NotFound();
                if (!authorized) return Forbid();
                if (existing == null) return NotFound();

                var updated = await _mediaService.UpdateAsync(id, updateMediaDto);

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
            var (existing, exists, authorized) = await TryAuthorizeMediaAsync(id);
            if (!exists) return NotFound();
            if (!authorized) return Forbid();
            if (existing == null) return NotFound();

            var deleted = await _mediaService.DeleteAsync(id);

            return deleted ? NoContent() : NotFound();
        }
    }
}
