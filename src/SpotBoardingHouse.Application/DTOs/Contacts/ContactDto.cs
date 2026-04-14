namespace SpotBoardingHouse.Application.DTOs.Contacts
{
    public class ContactDto
    {
        public Guid Id { get; set; }
        public Guid BoardingHouseId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Network { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
