using System.ComponentModel.DataAnnotations;

namespace SpotBoardingHouse.Application.DTOs.Amenities
{
    public class CreateAmenityDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Icon { get; set; }
    }
}
