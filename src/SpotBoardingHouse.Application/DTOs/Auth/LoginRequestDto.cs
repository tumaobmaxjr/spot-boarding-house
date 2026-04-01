using System.ComponentModel.DataAnnotations;

namespace SpotBoardingHouse.Application.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(12)]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;
    }
}
