using System.ComponentModel.DataAnnotations;

namespace SpotBoardingHouse.Application.DTOs.Users
{
    public class CreateUserDto
    {
        [Required]
        public Guid RoleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(12)]
        [MaxLength(500)]
        public string Password { get; set; } = string.Empty;
    }
}
