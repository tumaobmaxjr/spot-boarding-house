using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SpotBoardingHouse.Application.DTOs.Roles;
using SpotBoardingHouse.Application.Interfaces.Services;

namespace SpotBoardingHouse.Api.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetAll()
        {
            var roles = await _roleService.GetAllAsync();
            return Ok(roles);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<RoleDto>> GetById(Guid id)
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            return Ok(role);
        }

        [HttpPost]
        public async Task<ActionResult<RoleDto>> Create([FromBody] CreateRoleDto createRoleDto)
        {
            try
            {
                var role = await _roleService.CreateAsync(createRoleDto);
                return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<RoleDto>> Update(Guid id, [FromBody] UpdateRoleDto updateRoleDto)
        {
            try
            {
                var updated = await _roleService.UpdateAsync(id, updateRoleDto);
                if (updated == null)
                {
                    return NotFound();
                }

                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _roleService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
