using SpotBoardingHouse.Application.DTOs.Users;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Application.Interfaces.Services;
using SpotBoardingHouse.Domain.Entity;

namespace SpotBoardingHouse.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToDto);
        }

        public async Task<UserDto?> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : MapToDto(user);
        }

        public async Task<UserDto> CreateAsync(CreateUserDto createUserDto)
        {
            var normalizedEmail = createUserDto.Email.Trim().ToLowerInvariant();

            if (await _userRepository.EmailExistsAsync(normalizedEmail))
            {
                throw new InvalidOperationException($"User with email '{createUserDto.Email}' already exists.");
            }

            if (!await _userRepository.RoleExistsAsync(createUserDto.RoleId))
            {
                throw new InvalidOperationException("Role does not exist.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                RoleId = createUserDto.RoleId,
                FirstName = createUserDto.FirstName.Trim(),
                LastName = createUserDto.LastName.Trim(),
                Email = normalizedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
                CreatedAt = DateTime.UtcNow,
            };

            var created = await _userRepository.AddAsync(user);

            return MapToDto(created);
        }

        public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto updateUserDto)
        {
            var existing = await _userRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(updateUserDto.Email))
            {
                var normalizedEmail = updateUserDto.Email.Trim().ToLowerInvariant();
                if (await _userRepository.EmailExistsAsync(normalizedEmail, id))
                {
                    throw new InvalidOperationException($"User with email '{updateUserDto.Email}' already exists.");
                }

                existing.Email = normalizedEmail;
            }

            if (!string.IsNullOrWhiteSpace(updateUserDto.FirstName))
            {
                existing.FirstName = updateUserDto.FirstName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(updateUserDto.LastName))
            {
                existing.LastName = updateUserDto.LastName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(updateUserDto.Password))
            {
                existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUserDto.Password);
            }

            if (updateUserDto.RoleId.HasValue)
            {
                if (!await _userRepository.RoleExistsAsync(updateUserDto.RoleId.Value))
                {
                    throw new InvalidOperationException("Role does not exist.");
                }

                existing.RoleId = updateUserDto.RoleId.Value;
            }

            existing.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(existing);

            return MapToDto(existing);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var exists = await _userRepository.ExistsAsync(id);
            if (!exists)
            {
                return false;
            }

            await _userRepository.DeleteAsync(id);

            return true;
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                RoleId = user.RoleId,
                RoleName = user.Role.Name,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}
