using System.ComponentModel.DataAnnotations;

namespace SpotBoardingHouse.Application.DTOs.RoomOffers
{
    public class CreateRoomOfferDto
    {
        [Required]
        public Guid RoomId { get; set; }

        [Required]
        public Guid RoomTypeId { get; set; }

        [Required]
        public int Capacity { get; set; }

        [Required]
        public decimal MinPrice { get; set; }

        [Required]
        public decimal MaxPrice { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}
