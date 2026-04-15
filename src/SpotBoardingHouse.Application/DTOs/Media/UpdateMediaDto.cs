using System.ComponentModel.DataAnnotations;

namespace SpotBoardingHouse.Application.DTOs.Media
{
    public class UpdateMediaDto
    {
        [MaxLength(500)]
        public string? Url { get; set; }

        public bool? IsPrimary { get; set; }

        public int? DisplayOrder { get; set; }
    }
}
