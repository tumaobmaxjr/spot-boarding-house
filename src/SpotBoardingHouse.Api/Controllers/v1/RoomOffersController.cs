using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBoardingHouse.Application.DTOs.RoomOffers;
using SpotBoardingHouse.Application.DTOs.Rooms;
using SpotBoardingHouse.Application.Interfaces.Services;

namespace SpotBoardingHouse.Api.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Owner,Admin")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class RoomOffersController : ControllerBase
    {
        private readonly IRoomOfferService _roomOfferService;
        private readonly IRoomService _roomService;
        private readonly IBoardingHouseService _boardingHouseService;

        public RoomOffersController(
            IRoomOfferService roomOfferService,
            IRoomService roomService,
            IBoardingHouseService boardingHouseService)
        {
            _roomOfferService = roomOfferService;
            _roomService = roomService;
            _boardingHouseService = boardingHouseService;
        }

        private bool TryGetCurrentUserId(out Guid userId)
        {
            userId = Guid.Empty;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out userId);
        }

        private async Task<(RoomDto? Room, bool Exists, bool Authorized)> TryAuthorizeByRoomIdAsync(Guid roomId)
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
                return (null, false, false); // treat missing parent as not found
            }

            return (room, true, boardingHouse.UserId == userId);
        }

        [HttpGet("room/{roomId:guid}")]
        public async Task<ActionResult<IEnumerable<RoomOfferDto>>> GetAll(Guid roomId)
        {
            var (_, exists, authorized) = await TryAuthorizeByRoomIdAsync(roomId);

            if (!exists) return NotFound();
            if (!authorized) return Forbid();

            var roomOffers = await _roomOfferService.GetAllByRoomIdAsync(roomId);
            return Ok(roomOffers);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<RoomOfferDto>> GetById(Guid id)
        {
            var roomOffer = await _roomOfferService.GetByIdAsync(id);
            if (roomOffer == null)
            {
                return NotFound();
            }

            var (_, exists, authorized) = await TryAuthorizeByRoomIdAsync(roomOffer.RoomId);

            if (!exists) return NotFound();
            if (!authorized) return Forbid();

            return Ok(roomOffer);
        }

        [HttpPost]
        public async Task<ActionResult<RoomOfferDto>> Create([FromBody] CreateRoomOfferDto createRoomOfferDto)
        {
            try
            {
                var (_, exists, authorized) = await TryAuthorizeByRoomIdAsync(createRoomOfferDto.RoomId);

                if (!exists) return NotFound();
                if (!authorized) return Forbid();

                var roomOffer = await _roomOfferService.CreateAsync(createRoomOfferDto);
                return CreatedAtAction(nameof(GetById), new { id = roomOffer.Id }, roomOffer);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<RoomOfferDto>> Update(Guid id, [FromBody] UpdateRoomOfferDto updateRoomOfferDto)
        {
            try
            {
                var existing = await _roomOfferService.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound();
                }

                var (_, exists, authorized) = await TryAuthorizeByRoomIdAsync(existing.RoomId);

                if (!exists) return NotFound();
                if (!authorized) return Forbid();

                var roomOffer = await _roomOfferService.UpdateAsync(id, existing.RoomId, updateRoomOfferDto);
                return roomOffer == null ? NotFound() : Ok(roomOffer);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _roomOfferService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            var (_, exists, authorized) = await TryAuthorizeByRoomIdAsync(existing.RoomId);

            if (!exists) return NotFound();
            if (!authorized) return Forbid();

            var deleted = await _roomOfferService.DeleteAsync(id, existing.RoomId);
            return deleted ? NoContent() : NotFound();
        }

    }
}
