using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBoardingHouse.Application.DTOs.RoomOffers;
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

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userIdClaim!);
        }

        [HttpGet("room/{roomId:guid}")]
        public async Task<ActionResult<IEnumerable<RoomOfferDto>>> GetAll(Guid roomId)
        {
            var room = await _roomService.GetByIdAsync(roomId);
            if (room == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var userId = GetCurrentUserId();
                var boardingHouse = await _boardingHouseService.GetByIdAsync(room.BoardingHouseId);

                if (boardingHouse == null)
                {
                    return NotFound();
                }

                if (boardingHouse.UserId != userId)
                {
                    return Forbid();
                }
            }

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

            var room = await _roomService.GetByIdAsync(roomOffer.RoomId);
            if (room == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var userId = GetCurrentUserId();

                var boardingHouse = await _boardingHouseService.GetByIdAsync(room.BoardingHouseId);
                if (boardingHouse == null)
                {
                    return NotFound();
                }

                if (boardingHouse.UserId != userId)
                {
                    return Forbid();
                }
            }

            return Ok(roomOffer);
        }

        [HttpPost]
        public async Task<ActionResult<RoomOfferDto>> Create([FromBody] CreateRoomOfferDto createRoomOfferDto)
        {
            try
            {
                var room = await _roomService.GetByIdAsync(createRoomOfferDto.RoomId);
                if (room == null)
                {
                    return NotFound();
                }

                if (!User.IsInRole("Admin"))
                {
                    var userId = GetCurrentUserId();
                    var boardingHouse = await _boardingHouseService.GetByIdAsync(room.BoardingHouseId);

                    if (boardingHouse == null)
                    {
                        return NotFound();
                    }

                    if (boardingHouse.UserId != userId)
                    {
                        return Forbid();
                    }
                }

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

                var room = await _roomService.GetByIdAsync(existing.RoomId);
                if (room == null)
                {
                    return NotFound();
                }

                if (!User.IsInRole("Admin"))
                {
                    var userId = GetCurrentUserId();
                    var boardingHouse = await _boardingHouseService.GetByIdAsync(room.BoardingHouseId);

                    if (boardingHouse == null)
                    {
                        return NotFound();
                    }

                    if (boardingHouse.UserId != userId)
                    {
                        return Forbid();
                    }
                }



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

            var room = await _roomService.GetByIdAsync(existing.RoomId);
            if (room == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var userId = GetCurrentUserId();
                var boardingHouse = await _boardingHouseService.GetByIdAsync(room.BoardingHouseId);

                if (boardingHouse == null)
                {
                    return NotFound();
                }

                if (boardingHouse.UserId != userId)
                {
                    return Forbid();
                }
            }

            var deleted = await _roomOfferService.DeleteAsync(id, existing.RoomId);
            return deleted ? NoContent() : NotFound();
        }

    }
}
