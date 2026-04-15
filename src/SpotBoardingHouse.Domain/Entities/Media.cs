namespace SpotBoardingHouse.Domain.Entities
{
    public class Media : BaseEntity
    {
        public Guid? BoardingHouseId { get; set; }
        public BoardingHouse? BoardingHouse { get; set; }
        public Guid? RoomId { get; set; }
        public Room? Room { get; set; }

        public required string Url { get; set; }
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
    }
}
