using System.ComponentModel.DataAnnotations;

namespace SpotBoardingHouse.Application.DTOs.RoomTypes
{
    public class UpdateRoomTypeDto
    {
        [MaxLength(50)]
        public string? Name { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        public int? MinOccupants { get; set; }

        public int? MaxOccupants { get; set; }
    }
}
