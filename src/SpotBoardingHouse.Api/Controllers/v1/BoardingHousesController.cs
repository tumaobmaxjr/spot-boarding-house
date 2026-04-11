using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBoardingHouse.Application.DTOs.BoardingHouses;
using SpotBoardingHouse.Application.Interfaces.Services;

namespace SpotBoardingHouse.Api.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Owner,Admin")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class BoardingHousesController : ControllerBase
    {
        private readonly IBoardingHouseService _boardingHouseService;
        private readonly IUserService _userService;

        public BoardingHousesController(
            IBoardingHouseService boardingHouseService,
            IUserService userService)
        {
            _boardingHouseService = boardingHouseService;
            _userService = userService;
        }

        private bool TryGetCurrentUserId(out Guid userId)
        {
            userId = Guid.Empty;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out userId);
        }

        private async Task<(BoardingHouseDto? BoardingHouse, bool Exists, bool Authorized)> TryGetAuthorizedBoardingHouseAsync(Guid boardingHouseId)
        {
            var boardingHouse = await _boardingHouseService.GetByIdAsync(boardingHouseId);
            if (boardingHouse == null)
            {
                return (null, false, false);
            }

            // Admin path: ensure target owner is an Owner account
            if (User.IsInRole("Admin"))
            {
                var ownerUser = await _userService.GetByIdAsync(boardingHouse.UserId);
                var isOwnerAccount = ownerUser != null &&
                                     string.Equals(ownerUser.RoleName, "Owner", StringComparison.OrdinalIgnoreCase);
                return (boardingHouse, true, isOwnerAccount);
            }

            // Owner path
            if (!TryGetCurrentUserId(out var userId))
            {
                return (boardingHouse, true, false);
            }

            return (boardingHouse, true, boardingHouse.UserId == userId);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BoardingHouseDto>>> GetAll()
        {
            var isAdmin = User.IsInRole("Admin");

            if (isAdmin)
            {
                var allBoardingHouses = await _boardingHouseService.GetAllAsync();
                return Ok(allBoardingHouses);
            }

            if (!TryGetCurrentUserId(out Guid userId))
            {
                return Forbid();
            }

            var ownerBoardingHouses = await _boardingHouseService.GetAllByUserIdAsync(userId);

            return Ok(ownerBoardingHouses);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BoardingHouseDto>> GetById(Guid id)
        {
            var (boardingHouse, exists, authorized) = await TryGetAuthorizedBoardingHouseAsync(id);

            if (!exists) return NotFound();
            if (!authorized) return Forbid();
            if (boardingHouse == null) return NotFound();

            return Ok(boardingHouse);
        }

        [HttpPost]
        public async Task<ActionResult<BoardingHouseDto>> Create([FromBody] CreateBoardingHouseDto createBoardingHouseDto, [FromQuery] Guid? ownerId = null)
        {
            try
            {
                var isAdmin = User.IsInRole("Admin");

                if (!TryGetCurrentUserId(out var userId))
                {
                    return Forbid();
                }

                Guid targetOwnerId;

                if (isAdmin)
                {
                    if (!ownerId.HasValue)
                    {
                        return BadRequest(new { message = "Admin must specify an owner." });
                    }

                    if (ownerId.Value == userId)
                    {
                        return BadRequest(new { message = "Admin cannot create their own boarding house." });
                    }

                    var ownerUser = await _userService.GetByIdAsync(ownerId.Value);
                    var isOwnerAccount = ownerUser != null &&
                                         string.Equals(ownerUser.RoleName, "Owner", StringComparison.OrdinalIgnoreCase);

                    if (!isOwnerAccount)
                    {
                        return BadRequest(new { message = "Admin can only create boarding house for Owner accounts." });
                    }

                    targetOwnerId = ownerId.Value;
                }
                else
                {
                    targetOwnerId = userId;
                }

                var boardingHouse = await _boardingHouseService.CreateAsync(targetOwnerId, createBoardingHouseDto);

                return CreatedAtAction(nameof(GetById), new { id = boardingHouse.Id }, boardingHouse);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<BoardingHouseDto>> Update(Guid id, [FromBody] UpdateBoardingHouseDto updateBoardingHouseDto)
        {
            try
            {
                var (existing, exists, authorized) = await TryGetAuthorizedBoardingHouseAsync(id);

                if (!exists) return NotFound();
                if (!authorized) return Forbid();
                if (existing == null) return NotFound();

                var updated = await _boardingHouseService.UpdateAsync(id, existing.UserId, updateBoardingHouseDto);

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
            var (existing, exists, authorized) = await TryGetAuthorizedBoardingHouseAsync(id);

            if (!exists) return NotFound();
            if (!authorized) return Forbid();
            if (existing == null) return NotFound();

            var deleted = await _boardingHouseService.DeleteAsync(id, existing.UserId);

            return deleted ? NoContent() : NotFound();
        }
    }
}
