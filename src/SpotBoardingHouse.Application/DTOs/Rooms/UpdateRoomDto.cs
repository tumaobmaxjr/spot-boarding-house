using System.ComponentModel.DataAnnotations;

namespace SpotBoardingHouse.Application.DTOs.Rooms
{
    public class UpdateRoomDto
    {
        [MaxLength(50)]
        public string? RoomNumber { get; set; }

        public int? BaseCapacity { get; set; }

        public int? OccupiedSlots { get; set; }

        [MaxLength(500)]
        public string? AdditionalDescription { get; set; }
    }
}
