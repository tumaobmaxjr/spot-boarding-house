using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SpotBoardingHouse.Application.DTOs.Auth;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Application.Interfaces.Services;
using SpotBoardingHouse.Domain.Entity;

namespace SpotBoardingHouse.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest)
        {
            var normalizedEmail = loginRequest.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                return null;
            }

            return GenerateLoginResponse(user);
        }

        public async Task<LoginResponseDto?> RegisterAsync(RegisterRequestDto registerRequest)
        {
            var normalizedEmail = registerRequest.Email.Trim().ToLowerInvariant();

            if (await _userRepository.EmailExistsAsync(normalizedEmail))
            {
                throw new InvalidOperationException($"User with email '{registerRequest.Email}' already exists.");
            }

            var ownerRole = await _roleRepository.GetByNameAsync("Owner");
            if (ownerRole == null)
            {
                throw new InvalidOperationException("Owner role not found. Please run database seeeder.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                RoleId = ownerRole.Id,
                FirstName = registerRequest.FirstName.Trim(),
                LastName = registerRequest.LastName.Trim(),
                Email = normalizedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
            };

            var created = await _userRepository.AddAsync(user);

            // Re-fetch with role Role navigation to populate Role.Name
            var userWithRole = await _userRepository.GetByIdAsync(created.Id);
            if (userWithRole == null)
            {
                throw new InvalidOperationException("User was created but could not be retrieved.");
            }

            return GenerateLoginResponse(userWithRole);
        }

        private LoginResponseDto GenerateLoginResponse(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
            var expiresMinutes = int.Parse(_configuration["Jwt:ExpiresMinutes"] ?? "60");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.Name)
                }),
                Expires = DateTime.UtcNow.AddMinutes(expiresMinutes),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                ExpiresAtUtc = tokenDescriptor.Expires!.Value,
                UserId = user.Id,
                RoleId = user.RoleId,
                Role = user.Role.Name,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }
    }
}
