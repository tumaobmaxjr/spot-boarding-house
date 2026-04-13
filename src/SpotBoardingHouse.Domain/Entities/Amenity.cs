namespace SpotBoardingHouse.Domain.Entities
{
    public class Amenity : BaseEntity
    {
        public required string Name { get; set; }
        public string? Icon { get; set; }

        public ICollection<BoardingHouseAmenity> BoardingHouseAmenities { get; set; } = new List<BoardingHouseAmenity>();
        public ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
    }
}
