using System.ComponentModel.DataAnnotations;

namespace SpotBoardingHouse.Application.DTOs.Roles
{
    public class CreateRoleDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
