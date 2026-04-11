namespace SpotBoardingHouse.Domain.Entities
{
    public class RoomOffer : BaseEntity
    {
        public Guid RoomId { get; set; }
        public Room Room { get; set; } = null!;

        public Guid RoomTypeId { get; set; }
        public RoomType RoomType { get; set; } = null!;

        public int Capacity { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public bool IsAvailable { get; set; }
    }
}
