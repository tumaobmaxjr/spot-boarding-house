using System.ComponentModel.DataAnnotations;

namespace SpotBoardingHouse.Application.DTOs.BoardingHouses
{
    public class CreateBoardingHouseDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public string AllowedGender { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Address { get; set; } = string.Empty;

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [MaxLength(255)]
        public string? FacebookLink { get; set; }
    }
}
