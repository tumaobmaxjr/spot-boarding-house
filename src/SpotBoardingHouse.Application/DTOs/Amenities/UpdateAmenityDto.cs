using System.ComponentModel.DataAnnotations;

namespace SpotBoardingHouse.Application.DTOs.Amenities
{
    public class UpdateAmenityDto
    {
        [MaxLength(50)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? Icon { get; set; }
    }
}
