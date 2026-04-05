using System.ComponentModel.DataAnnotations;

namespace SpotBoardingHouse.Application.DTOs.Rooms
{
    public class CreateRoomDto
    {
        [Required]
        public Guid BoardingHouseId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        public int BaseCapacity { get; set; }

        public int OccupiedSlots { get; set; }

        [MaxLength(500)]
        public string? AdditionalDescription { get; set; }
    }
}
