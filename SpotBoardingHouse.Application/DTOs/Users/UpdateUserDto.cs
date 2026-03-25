using System.ComponentModel.DataAnnotations;

namespace SpotBoardingHouse.Application.DTOs.Users
{
    public class UpdateUserDto
    {
        public Guid? RoleId { get; set; }

        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string? LastName { get; set; }

        [EmailAddress]
        [MaxLength(255)]
        public string? Email { get; set; }

        [MinLength(12)]
        [MaxLength(500)]
        public string? Password { get; set; }
    }
}
