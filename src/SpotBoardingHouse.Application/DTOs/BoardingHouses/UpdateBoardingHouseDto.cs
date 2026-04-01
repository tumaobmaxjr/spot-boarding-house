using System.ComponentModel.DataAnnotations;

namespace SpotBoardingHouse.Application.DTOs.BoardingHouses
{
    public class UpdateBoardingHouseDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? AllowedGender { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [MaxLength(255)]
        public string? FacebookLink { get; set; }
    }
}
