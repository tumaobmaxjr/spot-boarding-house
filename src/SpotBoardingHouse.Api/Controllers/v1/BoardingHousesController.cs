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

        public BoardingHousesController(IBoardingHouseService boardingHouseService)
        {
            _boardingHouseService = boardingHouseService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userIdClaim!);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BoardingHouseDto>>> GetAll()
        {
            var userId = GetCurrentUserId();
            var boardingHouses = await _boardingHouseService.GetAllByUserIdAsync(userId);

            return Ok(boardingHouses);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BoardingHouseDto>> GetById(Guid id)
        {
            var boardingHouse = await _boardingHouseService.GetByIdAsync(id);
            if (boardingHouse == null)
            {
                return NotFound();
            }

            return Ok(boardingHouse);
        }

        [HttpPost]
        public async Task<ActionResult<BoardingHouseDto>> Create([FromBody] CreateBoardingHouseDto createBoardingHouseDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var boardingHouse = await _boardingHouseService.CreateAsync(userId, createBoardingHouseDto);

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
                var userId = GetCurrentUserId();
                var updated = await _boardingHouseService.UpdateAsync(id, userId, updateBoardingHouseDto);
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
            var userId = GetCurrentUserId();
            var deleted = await _boardingHouseService.DeleteAsync(id, userId);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
