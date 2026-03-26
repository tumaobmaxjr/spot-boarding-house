using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SpotBoardingHouse.Application.DTOs.Auth;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Application.Interfaces.Services;

namespace SpotBoardingHouse.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest)
        {
            var normalizedEmail = loginRequest.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);

            if (user == null)
            {
                return null;
            }

            var validPassword = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash);
            if (!validPassword)
            {
                return null;
            }

            var key = _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException("JWT key is not configured.");
            }

            var issuer = _configuration["Jwt:Issuer"] ?? "SpotBoardingHouse.Api";
            var audience = _configuration["Jwt:Audience"] ?? "SpotBoardingHouse.Api";
            var expiresMinutes = int.TryParse(_configuration["Jwt:ExpiresMinutes"], out var mins) ? mins : 60;

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(ClaimTypes.Role, user.Role?.Name ?? string.Empty)
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                ExpiresAtUtc = expiresAt,
                UserId = user.Id,
                RoleId = user.RoleId,
                Role = user.Role?.Name ?? string.Empty,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
            };
        }
    }
}
