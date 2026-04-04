namespace SpotBoardingHouse.Domain.Entity
{
    public class RoomType : BaseEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
