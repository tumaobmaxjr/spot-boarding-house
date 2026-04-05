namespace SpotBoardingHouse.Domain.Entities
{
    public class RoomType : BaseEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
