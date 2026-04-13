using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBoardingHouse.Application.DTOs.Amenities;
using SpotBoardingHouse.Application.Interfaces.Services;

namespace SpotBoardingHouse.Api.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AmenitiesController : ControllerBase
    {
        private readonly IAmenityService _amenityService;

        public AmenitiesController(IAmenityService amenityService)
        {
            _amenityService = amenityService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AmenityDto>>> GetAll()
        {
            var amenities = await _amenityService.GetAllAsync();

            return Ok(amenities);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<AmenityDto>> GetById(Guid id)
        {
            var amenity = await _amenityService.GetByIdAsync(id);
            return amenity == null ? NotFound() : Ok(amenity);
        }

        [HttpPost]
        public async Task<ActionResult<AmenityDto>> Create([FromBody] CreateAmenityDto createAmenityDto)
        {
            try
            {
                var amenity = await _amenityService.CreateAsync(createAmenityDto);
                return CreatedAtAction(nameof(GetById), new { id = amenity.Id }, amenity);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<AmenityDto>> Update(Guid id, [FromBody] UpdateAmenityDto updateAmenityDto)
        {
            try
            {
                var amenity = await _amenityService.UpdateAsync(id, updateAmenityDto);
                return amenity == null ? NotFound() : Ok(amenity);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _amenityService.DeleteAsync(id);

            return deleted ? NoContent() : NotFound();
        }
    }
}
