using SpotBoardingHouse.Application.DTOs.Auth;

namespace SpotBoardingHouse.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest);
        Task<LoginResponseDto?> RegisterAsync(RegisterRequestDto registerRequest);
    }
}
