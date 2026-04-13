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
    public class BoardingHouseAmenitiesController : ControllerBase
    {
        private readonly IBoardingHouseAmenityService _boardingHouseAmenityService;
        private readonly IBoardingHouseService _boardingHouseService;

        public BoardingHouseAmenitiesController(
            IBoardingHouseAmenityService boardingHouseAmenityService,
            IBoardingHouseService boardingHouseService)
        {
            _boardingHouseAmenityService = boardingHouseAmenityService;
            _boardingHouseService = boardingHouseService;
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

        [HttpGet("boarding-house/{boardingHouseId:guid}")]
        public async Task<ActionResult<IEnumerable<AmenityDto>>> GetByBoardingHouse(Guid boardingHouseId)
        {
            var (exists, authorized) = await TryAuthorizeBoardingHouseAsync(boardingHouseId);

            if (!exists) return NotFound();
            if (!authorized) return Forbid();

            var amenities = await _boardingHouseAmenityService.GetAmenitiesByBoardingHouseAsync(boardingHouseId);

            return Ok(amenities);
        }

        [HttpPost("boarding-house/{boardingHouseId:guid}")]
        public async Task<IActionResult> Attach(Guid boardingHouseId, [FromBody] AttachAmenityDto attachAmenityDto)
        {
            try
            {
                var (exists, authorized) = await TryAuthorizeBoardingHouseAsync(boardingHouseId);

                if (!exists) return NotFound();
                if (!authorized) return Forbid();

                await _boardingHouseAmenityService.AttachAsync(boardingHouseId, attachAmenityDto.AmenityId);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("boarding-house/{boardingHouseId:guid}/amenity/{amenityId:guid}")]
        public async Task<IActionResult> Detach(Guid boardingHouseId, Guid amenityId)
        {
            var (exists, authorized) = await TryAuthorizeBoardingHouseAsync(boardingHouseId);

            if (!exists) return NotFound();
            if (!authorized) return Forbid();

            var detached = await _boardingHouseAmenityService.DetachAsync(boardingHouseId, amenityId);

            return detached ? NoContent() : NotFound();
        }
    }
}
