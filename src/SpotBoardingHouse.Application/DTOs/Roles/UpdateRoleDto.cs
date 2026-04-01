using System.ComponentModel.DataAnnotations;

namespace SpotBoardingHouse.Application.DTOs.Roles
{
    public class UpdateRoleDto
    {
        [MaxLength(50)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
