using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBoardingHouse.Application.DTOs.Auth;
using SpotBoardingHouse.Application.Interfaces.Services;

namespace SpotBoardingHouse.Api.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto loginRequest)
        {
            var result = await _authService.LoginAsync(loginRequest);

            if (result == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            return Ok(result);
        }
    }
}
