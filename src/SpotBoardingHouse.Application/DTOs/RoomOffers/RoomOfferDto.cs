namespace SpotBoardingHouse.Application.DTOs.RoomOffers
{
    public class RoomOfferDto
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public Guid RoomTypeId { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public bool IsAvailable { get; set; }
        public string OfferStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
