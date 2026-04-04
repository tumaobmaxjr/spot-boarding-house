using System.ComponentModel.DataAnnotations;

namespace SpotBoardingHouse.Application.DTOs.RoomTypes
{
    public class CreateRoomTypeDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }
    }
}
