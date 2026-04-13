using System.ComponentModel.DataAnnotations;

namespace SpotBoardingHouse.Application.DTOs.Amenities
{
    public class AttachAmenityDto
    {
        [Required]
        public Guid AmenityId { get; set; }
    }
}
