namespace SpotBoardingHouse.Application.DTOs.Media
{
    public class MediaDto
    {
        public Guid Id { get; set; }
        public Guid? BoardingHouseId { get; set; }
        public Guid? RoomId { get; set; }

        public string Url { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
