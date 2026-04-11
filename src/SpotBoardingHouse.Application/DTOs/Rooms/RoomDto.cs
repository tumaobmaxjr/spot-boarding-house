namespace SpotBoardingHouse.Application.DTOs.Rooms
{
    public class RoomDto
    {
        public Guid Id { get; set; }
        public Guid BoardingHouseId { get; set; }
        public string BoardingHouseName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public int BaseCapacity { get; set; }
        public int OccupiedSlots { get; set; }
        public int RemainingSlots { get; set; }
        public string OccupancyStatus { get; set; } = string.Empty;
        public string? AdditionalDescription { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
