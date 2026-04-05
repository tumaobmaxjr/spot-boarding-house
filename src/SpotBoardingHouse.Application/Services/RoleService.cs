using SpotBoardingHouse.Application.DTOs.Roles;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Application.Interfaces.Services;
using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<IEnumerable<RoleDto>> GetAllAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Select(MapToDto);
        }

        public async Task<RoleDto?> GetByIdAsync(Guid id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            return role == null ? null : MapToDto(role);
        }


        public async Task<RoleDto> CreateAsync(CreateRoleDto createRoleDto)
        {
            if (await _roleRepository.NameExistsAsync(createRoleDto.Name))
            {
                throw new InvalidOperationException($"Role with name '{createRoleDto.Name}' already exists.");
            }

            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = createRoleDto.Name,
                Description = createRoleDto.Description,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _roleRepository.AddAsync(role);
            return MapToDto(created);
        }

        public async Task<RoleDto?> UpdateAsync(Guid id, UpdateRoleDto updateRoleDto)
        {
            var existing = await _roleRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(updateRoleDto.Name))
            {
                if (await _roleRepository.NameExistsAsync(updateRoleDto.Name, id))
                {
                    throw new InvalidOperationException($"Role with name '{updateRoleDto.Name}' already exists.");
                }
                existing.Name = updateRoleDto.Name;
            }

            if (updateRoleDto.Description != null)
            {
                existing.Description = updateRoleDto.Description;
            }

            existing.UpdatedAt = DateTime.UtcNow;

            await _roleRepository.UpdateAsync(existing);
            return MapToDto(existing);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var exists = await _roleRepository.ExistsAsync(id);
            if (!exists)
            {
                return false;
            }

            await _roleRepository.DeleteAsync(id);
            return true;
        }

        private static RoleDto MapToDto(Role role)
        {
            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt
            };
        }
    }
}
