namespace SpotBoardingHouse.Domain.Entities
{
    public class Room : BaseEntity
    {
        public Guid BoardingHouseId { get; set; }
        public BoardingHouse BoardingHouse { get; set; } = null!;

        public required string RoomNumber { get; set; }
        public int BaseCapacity { get; set; }
        public int OccupiedSlots { get; set; }
        public string? AdditionalDescription { get; set; }
    }
}
