using SpotBoardingHouse.Application.DTOs.Roles;

namespace SpotBoardingHouse.Application.Interfaces.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllAsync();
        Task<RoleDto?> GetByIdAsync(Guid id);
        Task<RoleDto> CreateAsync(CreateRoleDto createRoleDto);
        Task<RoleDto?> UpdateAsync(Guid id, UpdateRoleDto updateRoleDto);
        Task<bool> DeleteAsync(Guid id);
    }
}
