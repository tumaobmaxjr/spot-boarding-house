using System.ComponentModel.DataAnnotations;

namespace SpotBoardingHouse.Application.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(50)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
