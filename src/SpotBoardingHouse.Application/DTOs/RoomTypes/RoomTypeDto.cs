namespace SpotBoardingHouse.Application.DTOs.RoomTypes
{
    public class RoomTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MinOccupants { get; set; }
        public int MaxOccupants { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}
