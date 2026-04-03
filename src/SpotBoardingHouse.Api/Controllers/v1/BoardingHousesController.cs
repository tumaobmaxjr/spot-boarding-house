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

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userIdClaim!);
        }

        private async Task<bool> IsOwnerAccountAsync(Guid userId)
        {
            var user = await _userService.GetByIdAsync(userId);
            return user != null && string.Equals(user.RoleName, "Owner", StringComparison.OrdinalIgnoreCase);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BoardingHouseDto>>> GetAll()
        {
            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            var boardingHouses = isAdmin
                ? await _boardingHouseService.GetAllAsync()
                : await _boardingHouseService.GetAllByUserIdAsync(userId);

            return Ok(boardingHouses);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BoardingHouseDto>> GetById(Guid id)
        {
            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            var boardingHouse = await _boardingHouseService.GetByIdAsync(id);
            if (boardingHouse == null)
            {
                return NotFound();
            }

            if (isAdmin)
            {
                if (!await IsOwnerAccountAsync(boardingHouse.UserId))
                {
                    return Forbid();
                }
            }
            else if (boardingHouse.UserId != userId)
            {
                return Forbid();
            }

            return Ok(boardingHouse);
        }

        [HttpPost]
        public async Task<ActionResult<BoardingHouseDto>> Create([FromBody] CreateBoardingHouseDto createBoardingHouseDto, [FromQuery] Guid? ownerId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isAdmin = User.IsInRole("Admin");

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

                    if (!await IsOwnerAccountAsync(ownerId.Value))
                    {
                        return BadRequest(new { message = "Admin can only create boarding house for Owner accounts." });
                    }
                }
                else
                {
                    ownerId = userId;
                }

                var boardingHouse = await _boardingHouseService.CreateAsync(ownerId.Value, createBoardingHouseDto);

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
                var actorId = GetCurrentUserId();
                var isAdmin = User.IsInRole("Admin");

                var existing = await _boardingHouseService.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound();
                }

                Guid effectiveOwnerId;
                if (isAdmin)
                {
                    if (!await IsOwnerAccountAsync(existing.UserId))
                    {
                        return Forbid();
                    }

                    effectiveOwnerId = existing.UserId;
                }
                else
                {
                    if (existing.UserId != actorId)
                    {
                        return Forbid();
                    }

                    effectiveOwnerId = actorId;
                }

                var updated = await _boardingHouseService.UpdateAsync(id, effectiveOwnerId, updateBoardingHouseDto);
                if (updated == null)
                {
                    return NotFound();
                }

                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var actorId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            var existing = await _boardingHouseService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            Guid effectiveOwnerId;
            if (isAdmin)
            {
                if (!await IsOwnerAccountAsync(existing.UserId))
                {
                    return Forbid();
                }

                effectiveOwnerId = existing.UserId;
            }
            else
            {
                if (existing.UserId != actorId)
                {
                    return Forbid();
                }

                effectiveOwnerId = actorId;
            }

            var deleted = await _boardingHouseService.DeleteAsync(id, effectiveOwnerId);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
