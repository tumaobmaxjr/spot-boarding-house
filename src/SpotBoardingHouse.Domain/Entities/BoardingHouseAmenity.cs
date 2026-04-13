namespace SpotBoardingHouse.Domain.Entities
{
    public class BoardingHouseAmenity : BaseEntity
    {
        public Guid BoardingHouseId { get; set; }
        public BoardingHouse BoardingHouse { get; set; } = null!;

        public Guid AmenityId { get; set; }
        public Amenity Amenity { get; set; } = null!;
    }
}
