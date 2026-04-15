using System.ComponentModel.DataAnnotations;

namespace SpotBoardingHouse.Application.DTOs.Media
{
    public class CreateMediaDto
    {
        public Guid? BoardingHouseId { get; set; }
        public Guid? RoomId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Url { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        [Required]
        public int DisplayOrder { get; set; }
    }
}
